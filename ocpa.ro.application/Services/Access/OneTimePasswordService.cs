using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Constants;
using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Models.Authentication;
using ocpa.ro.domain.Models.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Auth = OPMFileUploader.Authentication;

namespace ocpa.ro.application.Services.Access;



public class OneTimePasswordService : BaseService, IOneTimePasswordService
{
    private readonly IApplicationDbContext _dbContext = null;
    private readonly IEmailService _emailService;
    private readonly AuthConfig _config;

    public OneTimePasswordService(IHostingEnvironmentService hostingEnvironment,
           ILogger logger,
           IEmailService emailService,
           ISystemSettingsService settingsService,
           IApplicationDbContext dbContext)
           : base(hostingEnvironment, logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _config = settingsService.AuthenticationSettings;
    }

    public (string err, User user) ValidateOneTimePassword(AuthenticateRequest req)
    {
        try
        {
            var loginId = (req.LoginId ?? "").ToLowerInvariant();
            var user = _dbContext.Users.FirstOrDefault(u => u.LoginId.ToLower() == loginId);

            if (user != null)
            {
                if (!user.Enabled)
                    return (AuthenticationErrors.AccountDisabled, user);

                var otp = _dbContext.OneTimePasswords.FirstOrDefault(o => o.UserId == user.Id);
                if (otp == null)
                    return (AuthenticationErrors.BadOtp, user);

                if (otp.Expiration.CompareTo(DateTime.UtcNow.ToString(AppConstants.DateTimeFormat)) < 0)
                {
                    _dbContext.Delete(otp); // Delete if expired
                    return (AuthenticationErrors.BadOtp, user);
                }

                var seed = Auth.getSeed(req.Password);
                var calc = Auth.calcHash(otp.Hash, seed);

                if (calc != req.Password)
                    return (AuthenticationErrors.BadOtp, user);

                _dbContext.Delete(otp); // Delete when succesfully used
                return (null, user);
            }

            return (AuthenticationErrors.BadCredentials, user);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return (AuthenticationErrors.BadOtp, null);
    }

    public async Task<bool> GenerateOneTimePassword(string loginId, string language)
    {
        bool success = false;

        try
        {
            loginId = (loginId ?? "").ToLowerInvariant();
            var user = _dbContext.Users.FirstOrDefault(u => u.LoginId.ToLower() == loginId);

            if (user != null)
            {
                var dbOtp = _dbContext.OneTimePasswords.FirstOrDefault(o => o.UserId == user.Id);
                if (dbOtp != null)
                    _dbContext.Delete(dbOtp); // Delete existing OTP when regenerating

                var otp = Guid.NewGuid().ToString().ToUpperInvariant().Substring(4, 9);
                await _emailService.SendOneTimePassword(user.EmailAddress, otp, language);

                dbOtp = new OneTimePassword
                {
                    UserId = user.Id,
                    Hash = Auth.hash(loginId, otp),
                    Expiration = DateTime.UtcNow.AddMinutes(_config.OTPDuration).ToString(AppConstants.DateTimeFormat),
                };

                success = _dbContext.Insert(dbOtp) > 0;
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return success;
    }
}
