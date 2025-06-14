﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Authentication;
using ocpa.ro.api.Models.Configuration;
using ocpa.ro.api.Persistence;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace ocpa.ro.api.Helpers.Authentication
{
    public interface IJwtTokenHelper
    {
        AuthenticationResponse GenerateJwtToken(User user);

    }

    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly AuthConfig _config;

        public JwtTokenHelper(IOptions<AuthConfig> config)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        }



        public AuthenticationResponse GenerateJwtToken(User user)
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

                Issuer = _config.Jwt.Issuer,
                Audience = _config.Jwt.Audience,

                Expires = DateTime.UtcNow.AddSeconds(_config.Jwt.Validity),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(JwtConfig.KeyBytes.ToArray()),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResponse
            {
                Validity = _config.Jwt.Validity,
                LoginId = user?.LoginId,
                AnonymizedEmail = StringUtility.AnonymizeEmail(user?.EmailAddress ?? ""),
                Token = tokenHandler.WriteToken(token),
                Type = user?.Type ?? default,
            };
        }
    }
}