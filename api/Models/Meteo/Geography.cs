using ocpa.ro.api.Models.Generic;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Models.Meteo
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

    public class GridCoordinates
    {
        public int R { get; set; }
        public int C { get; set; }
    }
}