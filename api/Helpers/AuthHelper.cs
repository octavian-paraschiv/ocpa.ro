using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ocpa.ro.api.Models;
using System.IO;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Helpers
{
    public interface IAuthHelper
    {
        User AuthorizeUser(AuthenticateRequest req);
        User SaveUser(User user);
        User GetUser(string loginId);
    }

    public class AuthHelper : IAuthHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment = null;
        private readonly SQLiteConnection _db = null;

        public AuthHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            string rootPath = Path.GetDirectoryName(_hostingEnvironment.ContentRootPath);
            string authDbFolder = Path.Combine(rootPath, "Content");
            string authDbFile = Path.Combine(authDbFolder, "auth.db");

            _db = new SQLiteConnection(authDbFile, SQLiteOpenFlags.ReadWrite);
        }

        public User AuthorizeUser(AuthenticateRequest req)
        {
            try
            {
                return _db.Get<User>(u => u.LoginId == req.LoginId && u.PasswordHash == req.Password);
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

        public User SaveUser(User user)
        {
            try
            {
                var dbu = _db.Get<User>(u => u.LoginId == user.LoginId);
                dbu ??= new User { Id = -1, LoginId = user.LoginId };

                dbu.Type = user.Type;
                dbu.PasswordHash = user.PasswordHash;

                if (dbu.Id < 0)
                    _db.Insert(dbu);
                else
                    _db.Update(dbu);

                return dbu;
            }
            catch { }

            return null;

        }
    }
}
