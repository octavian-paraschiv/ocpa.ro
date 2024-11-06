﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Geography;
using ocpa.ro.api.Models.Authentication;
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
    public interface IAuthHelper
    {
        User AuthorizeUser(AuthenticateRequest req);
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

    public class AuthHelper : BaseHelper, IAuthHelper
    {
        private readonly SQLiteConnection _db = null;
        private readonly IGeographyHelper _geographyHelper = null;

        public AuthHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IGeographyHelper geographyHelper)
            : base(hostingEnvironment, logger)
        {
            _geographyHelper = geographyHelper;

            string authDbFile = Path.Combine(_hostingEnvironment.ContentPath(), "auth.db");
            _db = new SQLiteConnection(authDbFile, SQLiteOpenFlags.ReadWrite);
        }

        public User AuthorizeUser(AuthenticateRequest req)
        {
            try
            {
                var loginId = (req.LoginId ?? "").ToLowerInvariant();
                var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);
                if (user?.PasswordHash?.Length > 0 && req.Password?.Length > 0)
                {
                    var seed = Auth.getSeed(req.Password);
                    var calc = Auth.calcHash(user.PasswordHash, seed);
                    return (calc == req.Password) ? user : null;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return null;
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

                dbu ??= new User { LoginId = loginId };

                dbu.Type = user.Type;
                dbu.PasswordHash = user.PasswordHash;

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

                if (_db.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
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
                        .Where(am => (am.UserId == user.Id) || (am.UserId == null && userType.Code == "ADM"));
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<AppMenu>();
        }

        public RegisteredDevice GetRegisteredDevice(string deviceId)
        {
            try
            {
                return _db.Table<RegisteredDevice>().Where(d => d.DeviceId == deviceId).SingleOrDefault();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
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

                var device = new RegisteredDevice
                {
                    DeviceId = deviceId,
                    LastLoginId = loginId,
                    LastLoginIpAddress = ipAddress,
                    LastLoginGeoLocation = geoLocationStr,
                    LastLoginTimestamp = DateTime.UtcNow,
                };

                _db.Insert(device);
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
    }
}
