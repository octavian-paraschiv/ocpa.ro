using ocpa.ro.api.Models.Authentication;
using ocpa.ro.domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Auth = OPMFileUploader.Authentication;

namespace ocpa.ro.api.Helpers.Authentication
{
    public interface IAuthHelperOtp
    {
        (string err, User user) ValidateOTP(AuthenticateRequest req);
        Task<bool> GenerateOTP(string loginId, string language);
    }

    public partial class AuthHelper
    {
        public (string err, User user) ValidateOTP(AuthenticateRequest req)
        {
            try
            {
                var loginId = (req.LoginId ?? "").ToLowerInvariant();
                var user = _dbContext.Users.FirstOrDefault(u => u.LoginId.ToLower() == loginId);

                if (user != null)
                {
                    if (!user.Enabled)
                        return ("ERR_ACCOUNT_DISABLED", user);

                    var otp = _dbContext.OneTimePasswords.FirstOrDefault(o => o.UserId == user.Id);
                    if (otp == null)
                        return ("ERR_BAD_OTP", user);

                    if (otp.Expiration.CompareTo(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")) < 0)
                    {
                        _dbContext.Delete(otp); // Delete if expired
                        return ("ERR_BAD_OTP", user);
                    }

                    var seed = Auth.getSeed(req.Password);
                    var calc = Auth.calcHash(otp.Hash, seed);

                    if (calc != req.Password)
                        return ("ERR_BAD_OTP", user);

                    _dbContext.Delete(otp); // Delete when succesfully used
                    return (null, user);
                }

                return ("ERR_BAD_CREDENTIALS", user);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return ("ERR_BAD_OTP", null);
        }

        public async Task<bool> GenerateOTP(string loginId, string language)
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
                    await _emailHelper.SendOneTimePassword(user.EmailAddress, otp, language);

                    dbOtp = new OneTimePassword
                    {
                        UserId = user.Id,
                        Hash = Auth.hash(loginId, otp),
                        Expiration = DateTime.UtcNow.AddMinutes(_config.OTPDuration).ToString("yyyy-MM-dd HH:mm:ss"),
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
}
