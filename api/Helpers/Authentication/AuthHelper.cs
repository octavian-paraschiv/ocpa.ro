using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Authentication;
using System.IO;
using System.Linq;
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
        User[] AllUsers();
    }

    public class AuthHelper : IAuthHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment = null;
        private readonly SQLiteConnection _db = null;

        public AuthHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            string authDbFile = Path.Combine(_hostingEnvironment.ContentPath(), "auth.db");
            _db = new SQLiteConnection(authDbFile, SQLiteOpenFlags.ReadWrite);
        }

        public User AuthorizeUser(AuthenticateRequest req)
        {
            try
            {
                var loginId = (req.LoginId ?? "").ToLowerInvariant();
                var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);
                if (user?.PasswordHash?.Length > 0 && req?.Password?.Length > 0)
                {
                    var seed = Auth.getSeed(req.Password);
                    var calc = Auth.calcHash(user.PasswordHash, seed);
                    return (calc == req.Password) ? user : null;
                }
            }
            catch { }

            return null;
        }

        public User GetUser(string loginId)
        {
            try
            {
                return _db.Get<User>(u => u.LoginId == loginId);
            }
            catch { }

            return null;
        }

        public User SaveUser(User user, out bool inserted)
        {
            User dbu = null;
            inserted = false;

            try
            {
                var loginId = (user.LoginId ?? "").ToLowerInvariant();

                dbu = _db.Get<User>(u => u.LoginId.ToLower() == loginId);
                dbu ??= new User { Id = -1, LoginId = loginId };

                if (user.Type != default)
                    dbu.Type = user.Type;

                dbu.PasswordHash = user.PasswordHash;

                if (dbu.Id < 0)
                {
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
            catch
            {
                dbu = null;
            }

            return dbu;

        }

        public int DeleteUser(string loginId)
        {
            try
            {
                var dbu = _db.Get<User>(u => u.LoginId == loginId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (_db.Delete(dbu) > 0)
                    return StatusCodes.Status204NoContent;
            }
            catch { }

            return StatusCodes.Status400BadRequest;
        }

        public User[] AllUsers()
        {
            return _db.Table<User>().Select(u => new User
            {
                Id = u.Id,
                LoginId = u.LoginId,
                Type = u.Type,
                PasswordHash = null,
            }).ToArray();
        }
    }
}
