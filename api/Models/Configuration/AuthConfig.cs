using System;
using System.Collections.Generic;
using System.Linq;

namespace ocpa.ro.api.Models.Configuration
{
    public class JwtConfig
    {
        public static readonly IEnumerable<byte> KeyBytes =
           Array.Empty<byte>()
           .Concat(Guid.NewGuid().ToByteArray())
           .Concat(Guid.NewGuid().ToByteArray())
           .Concat(Guid.NewGuid().ToByteArray())
           .ToList().AsReadOnly();

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Validity { get; set; }
    }

    public class AuthConfig
    {
        public static readonly string SectionName = "Authentication";

        public bool Disabled { get; set; } = false;

        public JwtConfig Jwt { get; set; }

        public bool UseOTP { get; set; } = false;
        public int OTPDuration { get; set; } = 180;
        public int PasswordHistoryLength { get; set; } = 5;
        public int MaxPasswordAge { get; set; } = 90;
        public int MaxLoginRetries { get; set; } = 5;
    }
}
