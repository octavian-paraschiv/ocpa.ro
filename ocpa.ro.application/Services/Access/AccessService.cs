using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ocpa.ro.common.Exceptions;
using ocpa.ro.domain.Abstractions;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Entities;
using ocpa.ro.domain.Models.Authentication;
using ocpa.ro.domain.Models.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Auth = OPMFileUploader.Authentication;

namespace ocpa.ro.application.Services.Access;



public partial class AccessService : BaseService, IAccessService
{
    private readonly IApplicationDbContext _dbContext = null;
    private readonly IGeographyService _geographyService = null;
    private readonly IEmailService _emailService;
    private readonly IAccessManagementService _accessManagementService;
    private readonly AuthConfig _config;

    public AccessService(IHostingEnvironmentService hostingEnvironment,
        IAccessManagementService accessManagementService,
        ILogger logger,
        IGeographyService geographyHelper,
        IEmailService emailService,
        IOptions<AuthConfig> config,
        IApplicationDbContext dbContext)
        : base(hostingEnvironment, logger)
    {
        _accessManagementService = accessManagementService ?? throw new ArgumentNullException(nameof(accessManagementService));
        _geographyService = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public (User user, bool useOTP) Authenticate(AuthenticateRequest req)
    {
        try
        {
            var user = GetUser(req.LoginId);
            if (user?.PasswordHash?.Length > 0 && req.Password?.Length > 0)
            {
                var seed = Auth.getSeed(req.Password);
                var calc = Auth.calcHash(user.PasswordHash, seed);

                if (calc == req.Password)
                {
                    user.LoginAttemptsRemaining = _config.MaxLoginRetries;
                }
                else
                {
                    user.LoginAttemptsRemaining = Math.Max(0, user.LoginAttemptsRemaining - 1);
                    user.Enabled = user.LoginAttemptsRemaining > 0;
                }

                bool useOTP = _config.UseOTP || (user?.UseOtp ?? false);
                return (SaveUser(user, out _), useOTP);
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return (null, false);
    }

    public User GetUser(string loginId)
    {
        try
        {
            loginId = (loginId ?? "").ToLowerInvariant();
            return _dbContext.Users.FirstOrDefault(u => u.LoginId.ToLower() == loginId);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return null;
    }

    public User SaveUser(User user, out bool inserted)
    {
        User dbu = null;
        inserted = false;

        try
        {
            var loginId = user.LoginId?.ToLowerInvariant();
            var id = user.Id;

            dbu = _dbContext.Users.FirstOrDefault(u =>
                (loginId == null || loginId == u.LoginId.ToLower()) &&
                (id <= 0 || id == u.Id));

            bool newUser = dbu == null;

            dbu ??= new User { LoginId = loginId, LoginAttemptsRemaining = _config.MaxLoginRetries };

            dbu.Type = user.Type;
            dbu.Enabled = user.Enabled;
            dbu.LoginAttemptsRemaining = user.LoginAttemptsRemaining;
            dbu.EmailAddress = user.EmailAddress;
            dbu.UseOtp = user.UseOtp;

            if (newUser)
            {
                dbu.PasswordHash = user.PasswordHash;
                if (_dbContext.Insert(dbu) > 0)
                    inserted = true;
                else
                    dbu = null;
            }
            else
            {
                if (user.PasswordHash?.Length > 0)
                    dbu.PasswordHash = user.PasswordHash;

                if (_dbContext.Update(dbu) <= 0)
                    dbu = null;
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            dbu = null;
        }

        return dbu;

    }

    public int DeleteUser(string loginId)
    {
        try
        {
            var dbu = _dbContext.Users.FirstOrDefault(u => u.LoginId.ToLower() == loginId.ToLower());
            if (dbu == null)
                return StatusCodes.Status404NotFound;

            _accessManagementService.DeleteAppsForUser(dbu.Id, false);

            if (_dbContext.Delete(dbu) > 0)
                return StatusCodes.Status200OK;
        }
        catch (ExtendedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return StatusCodes.Status400BadRequest;
    }

    public IEnumerable<User> AllUsers()
    {
        return [.. _dbContext.Users.Select(u => new User
        {
            Id = u.Id,
            LoginId = u.LoginId,
            Type = u.Type,

            EmailAddress = u.EmailAddress,
            UseOtp = u.UseOtp,
            LoginAttemptsRemaining = u.LoginAttemptsRemaining,

            PasswordHash = null,
            Enabled = u.Enabled && u.LoginAttemptsRemaining > 0,

        })];
    }

    public UserType GetUserType(int id = -1, string code = null)
    {
        try
        {
            return _dbContext.UserTypes.FirstOrDefault(ut =>
                (id < 0 || id == ut.Id) &&
                (code == null || code.ToLower() == ut.Code.ToLower())
            );
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return null;
    }

    public IEnumerable<PublicMenu> PublicMenus(string deviceId)
    {
        try
        {
            return [.. _dbContext.PublicMenus];
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return [];
    }

    public IEnumerable<AppMenu> ApplicationMenus(string deviceId, IIdentity identity)
    {
        try
        {
            if (identity is ClaimsIdentity claimsIdentity)
            {
                var uid = claimsIdentity.Claims
                    .AsEnumerable()
                   .Where(c => c.Type == "uid")
                   .Select(c => int.TryParse(c.Value, out int v) ? v : 0)
                   .FirstOrDefault();

                var user = _dbContext.Users.FirstOrDefault(u => u.Id == uid);
                if (user != null)
                {

                    var userType = _dbContext.UserTypes.FirstOrDefault(ut => ut.Id == user.Type);

                    if (userType != null)
                    {
                        return [.._dbContext.AppMenus
                            .AsEnumerable ()
                            .Where(am => am.UserId == user.Id || am.UserId == null && userType.Code == "ADM")
                            .DistinctBy(am => am.Id)];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return [];
    }


    public async Task RegisterDevice(string deviceId, string ipAddress, string loginId)
    {
        try
        {
            var geoLocation = _geographyService != null ?
                await _geographyService.GetGeoLocation(ipAddress) : null;

            var geoLocationStr = string.Equals(geoLocation?.Status, "success", StringComparison.OrdinalIgnoreCase) ?
                JsonSerializer.Serialize(geoLocation, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }) : "n/a";

            var device =
                GetRegisteredDevice(deviceId, out bool found) ??
                new RegisteredDevice { DeviceId = deviceId };

            device.LastLoginId = loginId;
            device.LastLoginIpAddress = ipAddress;
            device.LastLoginGeoLocation = geoLocationStr;
            device.LastLoginTimestamp = DateTime.UtcNow;

            if (found)
                _dbContext.Update(device);
            else
                _dbContext.Insert(device);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    public IEnumerable<RegisteredDevice> GetRegisteredDevices()
    {
        try
        {
            return [.. _dbContext.RegisteredDevices];
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return [];
    }

    public int DeleteRegisteredDevice(string deviceId)
    {
        try
        {
            var device = _dbContext.RegisteredDevices.FirstOrDefault(rd => rd.DeviceId == deviceId);
            if (device == null)
                return StatusCodes.Status404NotFound;

            if (_dbContext.Delete(device) > 0)
                return StatusCodes.Status200OK;
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return StatusCodes.Status400BadRequest;
    }

    public RegisteredDevice GetRegisteredDevice(string deviceId)
        => GetRegisteredDevice(deviceId, out _);

    private RegisteredDevice GetRegisteredDevice(string deviceId, out bool found)
    {
        RegisteredDevice device = null;

        try
        {
            device = _dbContext.RegisteredDevices.SingleOrDefault(d => d.DeviceId == deviceId);
        }
        catch (Exception ex)
        {
            LogException(ex);
            device = null;
        }

        found = device != null;
        return device;
    }

    public void GuardContentPath(IIdentity identity, string contentPath)
    {
        contentPath = contentPath ?? string.Empty;

        try
        {
            if (identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsAuthenticated)
            {
                var uid = claimsIdentity.Claims
                   .Where(c => c.Type == "uid")
                   .Select(c => int.TryParse(c.Value, out int v) ? v : 0)
                   .FirstOrDefault();

                var user = _dbContext.Users.First(u => u.Id == uid);
                var userType = _dbContext.UserTypes.First(ut => ut.Id == user.Type);

                switch (userType.Code)
                {
                    case "API":
                        throw new UnauthorizedAccessException();

                    case "APP":
                        {
                            var appMenus = _dbContext.AppMenus
                                .Where(am => am.UserId == user.Id).ToList();

                            if (appMenus?.Any(menu => menu.Url?.Length > 1 && contentPath.StartsWith(BuildWikiUrl(menu.Url), StringComparison.OrdinalIgnoreCase)) ?? false)
                                return;
                        }
                        break;

                    case "ADM":
                        return;
                }
            }

            var publicMenus = _dbContext.PublicMenus.ToList();

            if (publicMenus?.Any(menu => menu.Url?.Length > 1 && contentPath.StartsWith(BuildWikiUrl(menu.Url), StringComparison.OrdinalIgnoreCase)) ?? false)
                return;
        }
        catch { }

        throw new UnauthorizedAccessException();
    }

    private static string BuildWikiUrl(string menuUrl)
    {
        return menuUrl
            .TrimStart('/')
            .Replace("wiki-container", "wiki")
            .Replace("wiki-browser", "wiki")
            .TrimEnd('.');
    }
}

