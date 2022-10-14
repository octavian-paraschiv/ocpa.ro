using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Models
{
    public enum BuildType
    {
        Legacy,
        Experimental,
        Release,
    }

    public class BuildInfo
    {
        public string Title { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        public DateTime BuildDate { get; set; }

        public bool IsRelease { get; set; }

        public string Comment { get; set; }

        public string URL { get; set; }
    }
}
