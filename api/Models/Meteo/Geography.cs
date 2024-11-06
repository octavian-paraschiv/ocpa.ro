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

    public class GeoLocation
    {
        public string Status { get; set; }
        public string Continent { get; set; }
        public string ContinentCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Zip { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Timezone { get; set; }
        public int Offset { get; set; }
        public string Currency { get; set; }
        public string Isp { get; set; }
        public string Org { get; set; }
        public string As { get; set; }
        public string Asname { get; set; }
        public string Reverse { get; set; }
        public bool Mobile { get; set; }
        public bool Proxy { get; set; }
        public bool Hosting { get; set; }
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
