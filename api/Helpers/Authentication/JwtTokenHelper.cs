using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ocpa.ro.api.Models.Authentication;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ocpa.ro.api.Helpers.Authentication
{
    public interface IJwtTokenHelper
    {
        AuthenticateResponse GenerateJwtToken(User user);
    }

    public class JwtTokenHelper : IJwtTokenHelper
    {
        IConfiguration _configuration;

        public JwtTokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthenticateResponse GenerateJwtToken(User user)
        {
            // generate token that is valid for 1 hour
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user?.LoginId),
                    new Claim(ClaimTypes.Role, user?.Type.ToString())
                }),

                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],

                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticateResponse
            {
                Expires = tokenDescriptor.Expires.Value,
                LoginId = user?.LoginId,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}   