using ocpa.ro.api.Models.Authentication;
using System;
using System.Threading;
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
                var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);

                if (user != null)
                {
                    if (!user.Enabled)
                        return ("ERR_ACCOUNT_DISABLED", user);

                    var otp = _db.Find<OneTimePassword>(o => o.UserId == user.Id);
                    if (otp == null)
                        return ("ERR_BAD_OTP", user);

                    if (otp.Expiration <= DateTime.UtcNow)
                    {
                        _db.Delete(otp); // Delete if expired
                        return ("ERR_BAD_OTP", user);
                    }

                    var seed = Auth.getSeed(req.Password);
                    var calc = Auth.calcHash(otp.Hash, seed);

                    if (calc != req.Password)
                        return ("ERR_BAD_OTP", user);

                    _db.Delete(otp); // Delete when succesfully used
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
                var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);

                if (user != null)
                {
                    var dbOtp = _db.Find<OneTimePassword>(o => o.UserId == user.Id);
                    if (dbOtp != null)
                        _db.Delete(dbOtp); // Delete existing OTP when regenerating

                    var otp = Guid.NewGuid().ToString().ToUpperInvariant().Substring(4, 9);
                    await _emailHelper.SendOneTimePassword(user.EmailAddress, otp, language);

                    dbOtp = new OneTimePassword
                    {
                        Id = (_db.Table<OneTimePassword>().OrderByDescending(u => u.Id).FirstOrDefault()?.Id ?? 0) + 1,
                        UserId = user.Id,
                        Hash = Auth.hash(loginId, otp),
                        Expiration = DateTime.UtcNow.AddSeconds(_config.OTPDuration),
                    };

                    success = _db.Insert(dbOtp) > 0;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            finally
            {
                if (success)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            Thread.Sleep((_config.OTPDuration + 10) * 1000);
                            var user = _db.Get<User>(u => u.LoginId.ToLower() == loginId);
                            if (user != null)
                            {
                                var otpToDelete = _db.Find<OneTimePassword>(o => o.UserId == user.Id);
                                if (otpToDelete != null)
                                    _db.Delete(otpToDelete);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    });
                }

            }

            return success;
        }
    }
}
