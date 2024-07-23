using System;
using System.Collections.Generic;
using System.Linq;

namespace ocpa.ro.api.Models.Configuration
{
    public class JwtConfig
    {
        public static readonly string SectionName = "Jwt";

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
}
