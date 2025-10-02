using System.Collections.Generic;
using ThorusCommon.SQLite;

namespace ocpa.ro.domain.Models.Meteo;

public enum WindDirection
{
    W = 0,
    WSW,
    SW,
    SSW,
    S,
    SSE,
    SE,
    ESE,
    E,
    ENE,
    NE,
    NNE,
    N,
    NNW,
    NW,
    WNW
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
    public WindDirection WindDirection { get; set; }

    public int Precip { get; set; }
    public int SnowCover { get; set; }
    public int SoilRain { get; set; }
    public int Instability { get; set; }
    public int Fog { get; set; }

    public int Rain { get; set; }
    public int Snow { get; set; }

    public int P00 { get; set; }
    public int P01 { get; set; }
}

public class MeteoData : MeteoDbInfo
{
    public string Region { get; set; }
    public GridCoordinates GridCoordinates { get; set; }
    public Dictionary<string, MeteoDailyData> Data { get; set; }
}

public enum MeteoDbStatus
{
    Absent,
    Offline,
    Online
}

public class MeteoDbInfo
{
    public string Name { get; set; }
    public int Dbi { get; set; }
    public CalendarRange CalendarRange { get; set; }
    public int DataCount => CalendarRange?.Length ?? 0;
    public MeteoDbStatus Status { get; set; }
}
