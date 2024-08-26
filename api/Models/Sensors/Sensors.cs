using ocpa.ro.api.Models.Generic;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Models
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
