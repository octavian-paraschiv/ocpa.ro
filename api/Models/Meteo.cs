using ocpa.ro.api.Models;
using System.Collections.Generic;

namespace api.Controllers.Models
{
    public static class MeteoConstants
    {
        public const string DateFormat = "yyyy-MM-dd";
    }

	public class MeteoDailyData
	{
		public int TMinActual { get; set; }
		public int TMaxActual { get; set; }
		public int TMinNormal { get; set; }
		public int TMaxNormal { get; set; }
		public string Forecast { get; set; }
		public string TempFeel { get; set; }
	}

	public class MeteoData
	{
		public GridCoordinates GridCoordinates { get; set; }
		public CalendarRange CalendarRange { get; set; }
		public Dictionary<string, MeteoDailyData> Data { get; set; }
	}
}
