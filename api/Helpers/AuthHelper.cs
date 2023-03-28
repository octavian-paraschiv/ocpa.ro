using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using ThorusCommon.SQLite;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using ocpa.ro.api.Models;

namespace ocpa.ro.api.Helpers
{
    public interface IAuthHelper
    {
        public User AuthorizeUser(AuthenticateRequest req);
        public User SaveUser(User user);
        public User GetUser(string loginId);
    }

    public class AuthHelper : IAuthHelper
    {
        private IConfiguration _configuration = null;
        private IWebHostEnvironment _hostingEnvironment = null;

        private SQLiteConnection _db = null;

        public AuthHelper(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;

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
                if (dbu == null)
                    dbu = new User { Id = -1, LoginId = user.LoginId };

                dbu.SaltValue = user.SaltValue;
                dbu.FirstName = user.FirstName;
                dbu.LastName = user.LastName;
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
