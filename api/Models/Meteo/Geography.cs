using System.Collections.Generic;
using System.Text.Json.Serialization;
using ThorusCommon.IO.Converters;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Models.Geography
{
    public class CityDetail : City
    {
        public string RegionName { get; set; }
        public string RegionCode { get; set; }
    }
}

namespace ocpa.ro.api.Models.Internal
{
    public class Region
    {
        public string Name { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double MinLon { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double MaxLon { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double MinLat { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double MaxLat { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double GridResolution { get; set; }

        public List<City> Cities { get; set; }
    }

    public class City
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double Latitude { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double Longitude { get; set; }

        public bool Default { get; set; }
    }
}
