using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Exceptions;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Email;
using ocpa.ro.api.Helpers.Geography;
using ocpa.ro.api.Models.Authentication;
using ocpa.ro.api.Models.Configuration;
using ocpa.ro.api.Models.Menus;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using ThorusCommon.SQLite;
using Auth = OPMFileUploader.Authentication;

namespace ocpa.ro.api.Helpers.Authentication
{
    public interface IAuthHelper : IAuthHelperManagement, IAuthHelperOtp
    {
        (User user, bool useOTP) Authenticate(AuthenticateRequest req);

        User SaveUser(User user, out bool inserted);
        User GetUser(string loginId);
        int DeleteUser(string loginId);
        IEnumerable<User> AllUsers();
        IEnumerable<UserType> AllUserTypes();
        UserType GetUserType(int id = -1, string code = null);

        // ----
        IEnumerable<PublicMenu> PublicMenus(string deviceId);
        IEnumerable<AppMenu> ApplicationMenus(string deviceId, IIdentity identity);

        // ----
        IEnumerable<RegisteredDevice> GetRegisteredDevices();
        RegisteredDevice GetRegisteredDevice(string deviceId);
        Task RegisterDevice(string deviceId, string ipAddress, string loginId);
        int DeleteRegisteredDevice(string deviceId);
    }

    public partial class AuthHelper : BaseHelper
    {
        private readonly SQLiteConnection _db = null;
        private readonly IGeographyHelper _geographyHelper = null;
        private readonly IEmailHelper _emailHelper;
        private readonly AuthConfig _config;

        public AuthHelper(IWebHostEnvironment hostingEnvironment,
            ILogger logger,
            IGeographyHelper geographyHelper,
            IEmailHelper emailHelper,
            IOptions<AuthConfig> config)
            : base(hostingEnvironment, logger)
        {
            _geographyHelper = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
            _emailHelper = emailHelper ?? throw new ArgumentNullException(nameof(emailHelper));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

            string authDbFile = Path.Combine(_hostingEnvironment.ContentPath(), "auth.db");
            _db = new SQLiteConnection(authDbFile, SQLiteOpenFlags.ReadWrite);
        }

        public (User user, bool useOTP) Authenticate(AuthenticateRequest req)
        {
            try
            {
                var loginId = (req.LoginId ?? "").ToLowerInvariant();
                var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);
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

                    bool useOTP = _config.UseOTP || (user?.UseOTP ?? false);
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
                return _db.Get<User>(u => u.LoginId == loginId);
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

                dbu = _db.Table<User>().FirstOrDefault(u =>
                    (loginId == null || loginId == u.LoginId.ToLower()) &&
                    (id <= 0 || id == u.Id));

                bool newUser = (dbu == null);

                dbu ??= new User { LoginId = loginId, LoginAttemptsRemaining = _config.MaxLoginRetries };

                dbu.Type = user.Type;
                dbu.PasswordHash = user.PasswordHash;
                dbu.Enabled = user.Enabled;

                dbu.LoginAttemptsRemaining = user.LoginAttemptsRemaining;
                dbu.EmailAddress = user.EmailAddress;
                dbu.UseOTP = user.UseOTP;

                if (newUser)
                {
                    dbu.Id = (_db.Table<User>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1;

                    if (_db.Insert(dbu) > 0)
                        inserted = true;
                    else
                        dbu = null;
                }
                else
                {
                    if (_db.Update(dbu) <= 0)
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
                var dbu = _db.Get<User>(u => u.LoginId.ToLower() == loginId.ToLower());
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                DeleteAppsForUser(dbu.Id, false);

                if (_db.Delete(dbu) > 0)
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
            return _db.Table<User>().Select(u => new User
            {
                Id = u.Id,
                LoginId = u.LoginId,
                Type = u.Type,

                EmailAddress = u.EmailAddress,
                UseOTP = u.UseOTP,
                LoginAttemptsRemaining = u.LoginAttemptsRemaining,

                PasswordHash = null,
            });
        }

        public IEnumerable<UserType> AllUserTypes()
        {
            return _db.Table<UserType>();
        }

        public UserType GetUserType(int id = -1, string code = null)
        {
            try
            {
                return _db.Table<UserType>().FirstOrDefault(ut =>
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
                return _db.Table<PublicMenu>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<PublicMenu>();
        }

        public IEnumerable<AppMenu> ApplicationMenus(string deviceId, IIdentity identity)
        {
            try
            {
                if (identity is ClaimsIdentity claimsIdentity)
                {
                    var uid = claimsIdentity.Claims
                       .Where(c => c.Type == "uid")
                       .Select(c => int.TryParse(c.Value, out int v) ? v : 0)
                       .FirstOrDefault();

                    var user = _db.Table<User>().Where(u => u.Id == uid).First();
                    var userType = _db.Table<UserType>().Where(ut => ut.Id == user.Type).First();

                    return _db.Table<AppMenu>()
                        .Where(am => (am.UserId == user.Id) || (am.UserId == null && userType.Code == "ADM"))
                        .DistinctBy(am => am.Id);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<AppMenu>();
        }


        public async Task RegisterDevice(string deviceId, string ipAddress, string loginId)
        {
            try
            {
                var geoLocation = _geographyHelper != null ?
                    await _geographyHelper.GetGeoLocation(ipAddress) : null;

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
                    _db.Update(device);
                else
                {
                    device.Id = (_db.Table<RegisteredDevice>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1;
                    _db.Insert(device);
                }
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
                return _db.Table<RegisteredDevice>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<RegisteredDevice>();
        }

        public int DeleteRegisteredDevice(string deviceId)
        {
            try
            {
                var device = _db.Get<RegisteredDevice>(rd => rd.DeviceId == deviceId);
                if (device == null)
                    return StatusCodes.Status404NotFound;

                if (_db.Delete(device) > 0)
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
                device = _db.Table<RegisteredDevice>().Where(d => d.DeviceId == deviceId).SingleOrDefault();

            }
            catch (Exception ex)
            {
                LogException(ex);
                device = null;
            }

            found = (device != null);
            return device;
        }
    }
}

