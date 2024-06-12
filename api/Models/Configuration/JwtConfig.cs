using System;
using System.Linq;

namespace ocpa.ro.api.Models.Configuration
{
    public class JwtConfig
    {
        public static readonly string SectionName = "Jwt";

        public static readonly byte[] KeyBytes =
            Array.Empty<byte>()
            .Concat(Guid.NewGuid().ToByteArray())
            .Concat(Guid.NewGuid().ToByteArray())
            .Concat(Guid.NewGuid().ToByteArray())
            .ToArray();

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Validity { get; set; }
    }
}
