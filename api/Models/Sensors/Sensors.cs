using System.Collections.Generic;

namespace ocpa.ro.api.Models
{
    public class SensorDataCollection
    {
        public List<SensorData> SensorData { get; set; }
    }

    public class SensorData
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Humidity { get; set; }
    }
}
