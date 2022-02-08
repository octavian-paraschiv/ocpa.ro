using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPMedia.Backend.Models
{
    public class SensorData
    {
        [JsonProperty("t")]
        public double Temperature { get; set; }

        [JsonProperty("p")]
        public double Pressure { get; set; }

        [JsonProperty("h")]
        public double Humidity { get; set; }
    }

    public class SensorDataCollection
    {
        [JsonProperty("sensors")]
        public List<SensorData> SensorData { get; set; }
    }
}