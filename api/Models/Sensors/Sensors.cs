using System.Collections.Generic;
using System.Text.Json.Serialization;
using ThorusCommon.IO.Converters;

namespace ocpa.ro.api.Models.Sensors
{
    public class SensorDataCollection
    {
        public List<SensorData> SensorData { get; set; }
    }

    public class SensorData
    {
        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double Temperature { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double Pressure { get; set; }

        [JsonConverter(typeof(NumberTruncateJsonConverter<double>))]
        public double Humidity { get; set; }
    }
}
