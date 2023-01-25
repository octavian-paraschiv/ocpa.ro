using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ocpa.ro.api.Helpers
{
    public interface IAuthHelper
    {
        public string Authorize(HttpRequest request);
        public void Authorize(HttpRequest request, string body);
    }

    public class AuthHelper : IAuthHelper
    {
        private IConfiguration _configuration = null;
        private ITokenUtility _tokenUtility = null;  

        public AuthHelper(IConfiguration configuration, ITokenUtility tokenUtility)
        {
            _configuration = configuration;
            _tokenUtility = tokenUtility;
        }

        public void Authorize(HttpRequest request, string body)
        {
            var reqAuthHeader = Authorize(request);

            var reqSignature = request.Headers["X-Signature"];
            if (string.IsNullOrEmpty(reqSignature))
                ThrowUnauthorizedException("The X-Signature header is missing or invalid");

            var reqDateHeader = request.Headers["X-Date"];
            if (string.IsNullOrEmpty(reqDateHeader))
                ThrowUnauthorizedException("The X-Date header is missing or invalid");

            var requestId = request.Headers["X-Request-Id"];
            if (string.IsNullOrEmpty(requestId))
                ThrowUnauthorizedException("The X-Request-Id header is missing or invalid");

            if (!_tokenUtility.CheckIfActive(requestId, reqAuthHeader))
                ThrowUnauthorizedException("The X-Request-Id header is missing or invalid");

            // Validate the Date header format
            if (!DateTime.TryParseExact(reqDateHeader, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime reqDateTime))
                ThrowUnauthorizedException("The X-Date header is missing or invalid");

            // Validate the Date header actual value
            var offsetMinutes = (int)Math.Floor(Math.Abs(DateTime.UtcNow.Subtract(reqDateTime).TotalMinutes));
            if (offsetMinutes > 2)
                ThrowUnauthorizedException("Client and server date/time are not in sync");

            if (body.Length > 0)
            {
                var method = request.Method;
                var requestResource = request.Path.Value;
                string contentHash = string.Empty;
                string contentType = "application/json";

                using (SHA1 sha1 = SHA1.Create())
                {
                    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(body));
                    contentHash = Convert.ToBase64String(hash);
                }

                var stringToSign =
                    $"{method}\n" +
                    $"{contentHash}\n" +
                    $"{contentType}\n" +
                    $"{reqDateHeader}\n" +
                    $"{requestResource}";

                var calcSignature = GetHMACSHA1Hash(stringToSign, requestId);
                if (!string.Equals(reqSignature, calcSignature, StringComparison.Ordinal))
                    ThrowUnauthorizedException("Bad request signature");
            }
        }

        public string Authorize(HttpRequest request)
        {
            var reqAuthHeader = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(reqAuthHeader))
                ThrowUnauthorizedException("The Authorization header is missing or invalid");

            var user = _configuration["apiUserName"];
            var pass = _configuration["apiPassword"];

            using (SHA1 sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes($"{user}:{pass}"));
                var base64 = Convert.ToBase64String(hash);

                if (!string.Equals(reqAuthHeader, base64, StringComparison.Ordinal))
                    ThrowUnauthorizedException("The Authorization header is missing or invalid");
            }

            return reqAuthHeader;
        }

        private static void ThrowUnauthorizedException(string message)
        {
            throw new UnauthorizedAccessException(message);
        }

        private static string GetHMACSHA1Hash(string input, string key)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            using (var hmac = new HMACSHA1(keyBytes))
            {
                var hash = hmac.ComputeHash(inputBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
