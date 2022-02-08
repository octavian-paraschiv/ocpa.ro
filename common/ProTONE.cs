using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    [Flags]
    public enum BuildType
    {
        Legacy,
        Experimental,
        Release,
    }

    public class BuildInfo
    {
        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        [JsonProperty("Version")]
        public Version Version { get; set; }

        [JsonProperty("BuildDate")]
        public DateTime BuildDate { get; set; }

        [JsonProperty("IsRelease")]
        public bool IsRelease { get; set; }

        [JsonProperty("Comment")]
        public string Comment { get; set; }

        [JsonProperty("URL")]
        public string URL { get; set; }
    }

    public class Builds
    {
        public BuildInfo[] List;
    }
}
