using System.Text;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Models.Configuration
{
    public class JwtConfig
    {
        public static string SectionName = "Jwt";

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int Validity { get; set; }

        [JsonIgnore]
        public byte[] KeyBytes => Encoding.UTF8.GetBytes(Key);
    }
}
