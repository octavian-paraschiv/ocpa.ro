using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ocpa.ro.api.Models.Authentication;
using ocpa.ro.api.Models.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace ocpa.ro.api.Helpers.Authentication
{
    public interface IJwtTokenHelper
    {
        AuthenticateResponse GenerateJwtToken(User user);
    }

    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly JwtConfig _jwtConfig;

        public JwtTokenHelper(IOptions<JwtConfig> jwtConfigOptions)
        {
            _jwtConfig = jwtConfigOptions?.Value ?? throw new ArgumentNullException(nameof(jwtConfigOptions));
        }

        public AuthenticateResponse GenerateJwtToken(User user)
        {
            // generate token that is valid for 1 hour
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim("id", user?.LoginId ?? ""),
                    new Claim("uid", (user?.Id ?? 0).ToString()),
                    new Claim(ClaimTypes.Role, (user?.Type ?? 0).ToString())
                ]),

                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,

                Expires = DateTime.UtcNow.AddSeconds(_jwtConfig.Validity),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(JwtConfig.KeyBytes.ToArray()),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticateResponse
            {
                Expires = tokenDescriptor.Expires.Value,
                Validity = _jwtConfig.Validity,
                LoginId = user?.LoginId,
                Token = tokenHandler.WriteToken(token),
                Type = user?.Type ?? default,
            };
        }
    }
}