using ocpa.ro.domain.Models.Meteo;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IMeteoScalesService
{
    InstabilityScale Instability { get; }
    PrecipScale Precip { get; }
    FogScale Fog { get; }
    WindScale Wind { get; }
    PrecipBoundariesScale Boundaries { get; }
    TemperatureScale Temperature { get; }
}