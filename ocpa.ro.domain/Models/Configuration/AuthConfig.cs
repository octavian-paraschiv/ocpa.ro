using System;
using System.Collections.Generic;
using System.Linq;

namespace ocpa.ro.domain.Models.Configuration
{
    public class JwtConfig
    {
        public static readonly IEnumerable<byte> KeyBytes =
           Array.Empty<byte>()
           .Concat(Guid.NewGuid().ToByteArray())
           .Concat(Guid.NewGuid().ToByteArray())
           .Concat(Guid.NewGuid().ToByteArray())
           .ToList().AsReadOnly();

        public string Issuer { get; set; } = "ocpa.ro";
        public string Audience { get; set; } = "ocpa.ro";
        public int Validity { get; set; } = 900;
    }

    public class AuthConfig
    {
        public bool Disabled { get; set; } = false;
        public JwtConfig Jwt { get; set; } = new();
        public bool UseOTP { get; set; } = false;
        public int OTPDuration { get; set; } = 3;
        public int PasswordHistoryLength { get; set; } = 5;
        public int MaxPasswordAge { get; set; } = 90;
        public int MaxLoginRetries { get; set; } = 5;
    }
}
