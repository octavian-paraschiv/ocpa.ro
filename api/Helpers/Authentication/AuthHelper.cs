using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ocpa.ro.api.Models.Authentication;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ThorusCommon.SQLite;

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

        static readonly Assembly _extAsm = load();

        private static Assembly load()
        {
            using (var stream = typeof(ThorusCommon.IO.Constants).Assembly.GetManifestResourceStream("ThorusCommon.IO.ext.dll"))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        }

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
                var loginId = (req.LoginId ?? "").ToLowerInvariant();
                var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);
                if (user?.PasswordHash?.Length > 0 && req?.Password?.Length > 0)
                {
                    var seed = ext("j", req.Password);
                    var calc = ext("k", user.PasswordHash, seed);
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

        private static string ext(string method, params object[] args)
        {
            try
            {
                var field = _extAsm?.GetType("ext")?.GetField(method, BindingFlags.Static | BindingFlags.Public);
                switch ((args?.Length).GetValueOrDefault())
                {
                    case 0:
                        return (field.GetValue(null) as Func<string>)();
                    case 1:
                        return (field.GetValue(null) as Func<string, string>)(args[0] as string);
                    case 2:
                        return (field.GetValue(null) as Func<string, string, string>)(args[0] as string, args[1] as string);
                }
            }
            catch { }

            return null;
        }
    }
}
