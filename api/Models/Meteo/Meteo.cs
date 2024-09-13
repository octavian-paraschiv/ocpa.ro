using System.Collections.Generic;

namespace ocpa.ro.api.Models.Meteo
{
    public static class MeteoConstants
    {
        public const string DateFormat = "yyyy-MM-dd";
    }

    public class MeteoDailyData
    {
        public string Date { get; set; }
        public int TMinActual { get; set; }
        public int TMaxActual { get; set; }
        public int TMinNormal { get; set; }
        public int TMaxNormal { get; set; }
        public string Forecast { get; set; }
        public string TempFeel { get; set; }

        public List<string> Hazards { get; set; } = [];
        public int Wind { get; set; }
        public int Precip { get; set; }
        public int SnowCover { get; set; }
        public int SoilRain { get; set; }
        public int Instability { get; set; }
        public int Fog { get; set; }

        public int Rain { get; set; }
        public int Snow { get; set; }

        public string WindDirection { get; set; }

        public int P00 { get; set; }
        public int P01 { get; set; }
    }

    public class MeteoData : MeteoDbInfo
    {
        public GridCoordinates GridCoordinates { get; set; }
        public Dictionary<string, MeteoDailyData> Data { get; set; }
    }

    public class MeteoDbInfo
    {
        public string Name { get; set; }
        public int Dbi { get; set; }
        public CalendarRange CalendarRange { get; set; }
        public int DataCount => CalendarRange?.Length ?? 0;

        public bool Online => string.Equals(Name, "Snapshot.db3", System.StringComparison.OrdinalIgnoreCase);

        // By convention, databases uploaded via Thorus (which we can't override) are always uploaded as Preview3.db3
        public bool Modifyable => !Online && !string.Equals(Name, "Preview3.db3", System.StringComparison.OrdinalIgnoreCase);
    }
}
