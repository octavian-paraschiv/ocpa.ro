using ocpa.ro.api.Helpers.Meteo;
using ThorusCommon.IO;

namespace ocpa.ro.api.Models.Meteo;

public class MeteoScales : IMeteoScalesHelper
{
    public InstabilityScale Instability { get; set; } = new();
    public PrecipScale Precip { get; set; } = new();
    public FogScale Fog { get; set; } = new();
    public WindScale Wind { get; set; } = new();
    public PrecipBoundariesScale Boundaries { get; set; } = new();
    public TemperatureScale Temperature { get; set; } = new();
}

public class TemperatureScale
{
    public float Hot { get; set; } = 36f;
    public float Frost { get; set; } = -12f;
    public float Colder { get; set; } = -8f;
    public float Cold { get; set; } = -4f;
    public float Warm { get; set; } = 4f;
    public float Warmer { get; set; } = 8f;
}

public class PrecipScale
{
    public float Weak { get; set; } = 10f;
    public float Moderate { get; set; } = 20f;
    public float Heavy { get; set; } = 35f;
    public float Extreme { get; set; } = 50f;
}

public class WindScale
{
    public float Weak { get; set; } = 10f;
    public float Moderate { get; set; } = 20f;
    public float Heavy { get; set; } = 50f;
    public float Extreme { get; set; } = 80f;
}

public class FogScale
{
    public float Weak { get; set; } = 80f;
    public float Moderate { get; set; } = 40f;
    public float Heavy { get; set; } = 20f;
    public float Extreme { get; set; } = 10f;
}

public class InstabilityScale
{
    public float Weak { get; set; } = -6f;
    public float Moderate { get; set; } = -2f;
    public float Heavy { get; set; } = 2f;
    public float Extreme { get; set; } = 6f;
}

public class PrecipBoundariesScale : IPrecipTypeBoundaries
{
    public float MaxTeForSolidPrecip { get; set; } = -2.5f;
    public float MinTeForLiquidPrecip { get; set; } = 2.5f;
    public float MaxTsForFreezing { get; set; } = -0.1f;
    public float MinTsForMelting { get; set; } = 0.1f;
    public float MaxFreezingRainDelta { get; set; } = 5f;
}

