using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Models.Meteo;
using System;
using System.Collections.Generic;
using ThorusCommon.IO;
using ThorusCommon.SQLite;

namespace ocpa.ro.application.Services;

public class WeatherTypeService : IWeatherTypeService
{
    private readonly WindDirection[] _windDirections = Enum.GetValues<WindDirection>();
    private IMeteoScalesService _meteoScalesService;

    public WeatherTypeService(IMeteoScalesService meteoScalesService)
    {
        _meteoScalesService = meteoScalesService ?? throw new ArgumentNullException(nameof(meteoScalesService));
    }

    public string GetWeatherType(Data meteoData, int windSpeed, List<string> risks)
    {
        float precip = meteoData.C_00;
        float inst = -meteoData.L_00;
        float fog = meteoData.F_SI;

        var inst_threshold = _meteoScalesService.Instability.Weak;
        var inst_heavy = _meteoScalesService.Instability.Heavy;

        var precip_weak = _meteoScalesService.Precip.Weak;
        var precip_moderate = _meteoScalesService.Precip.Moderate;
        var precip_heavy = _meteoScalesService.Precip.Heavy;
        var precip_extreme = _meteoScalesService.Precip.Extreme;

        var fog_weak = _meteoScalesService.Fog.Weak;
        var fog_moderate = _meteoScalesService.Fog.Moderate;
        var fog_heavy = _meteoScalesService.Fog.Heavy;
        var fog_extreme = _meteoScalesService.Fog.Extreme;

        var wind_weak = _meteoScalesService.Wind.Weak;
        var wind_moderate = _meteoScalesService.Wind.Moderate;
        var wind_heavy = _meteoScalesService.Wind.Heavy;
        var wind_extreme = _meteoScalesService.Wind.Extreme;

        string intensity = "00";
        string type = GetPrecipType(meteoData);
        if (type == "rain" && inst >= inst_threshold)
            type = "inst";

        string actualPrecipType = type;
        if (type == "inst" || type == "ice")
            actualPrecipType = "rain";
        if (type == "mix")
            actualPrecipType = "sleet";

        if (precip >= precip_extreme)
        {
            intensity = "04";
            risks.Add($"heavy_{actualPrecipType}");
        }
        else if (precip >= precip_heavy)
        {
            intensity = "03";
            risks.Add($"intense_{actualPrecipType}");
        }
        else if (precip >= precip_moderate)
            intensity = "02";
        else if (precip >= precip_weak)
            intensity = "01";

        if (fog <= fog_extreme)
            risks.Add("very_thick_fog");
        else if (fog <= fog_heavy)
            risks.Add("thick_fog");

        if (windSpeed >= wind_extreme)
            risks.Add("heavy_wind");
        else if (windSpeed >= wind_heavy)
            risks.Add("strong_wind");

        if (intensity != "00")
        {
            if (inst >= inst_heavy)
                risks.Add("squalls_hail");

            if (type == "ice")
                risks.Add("freezing_rain");

            return $"{intensity}_{type}";
        }

        if (fog <= fog_extreme)
            return "04_fog";
        else if (fog <= fog_heavy)
            return "03_fog";
        else if (fog <= fog_moderate)
            return "02_fog";
        else if (fog <= fog_weak)
            return "01_fog";

        if (windSpeed >= wind_extreme)
            return "04_wind";
        else if (windSpeed >= wind_heavy)
            return "03_wind";
        else if (windSpeed >= wind_moderate)
            return "02_wind";
        else if (windSpeed >= wind_weak)
            return "01_wind";

        return "00";
    }

    public (int windSpeed, WindDirection direction) GetWind(Data meteoData)
    {
        var w00 = meteoData.W_00;
        var w01 = meteoData.W_01;
        var w10 = meteoData.W_10;
        var w11 = meteoData.W_11;

        var slice = Math.Floor(4 * (w10 + w11) / Math.PI);
        var idx = (int)Math.Min(_windDirections.Length - 1, Math.Max(0, slice));

        int windSpeed = (int)Math.Round(7.6f * (w00 + w01));
        WindDirection windDirection = _windDirections[idx];

        return (windSpeed, windDirection);
    }

    public string GetPrecipType(Data meteoData)
    {
        var te = meteoData.T_TE;
        var ts = meteoData.T_TS;
        var t01 = meteoData.T_01;

        return PrecipTypeComputer.Compute(
            // Actual temperatures
            te, ts, t01,

            // Boundary temperatures as read from config file
            _meteoScalesService.Boundaries,

            // Computed precip type: snow
            () => "snow",

            // Computed precip type: rain
            () => "rain",

            // Computed precip type: freezing rain
            () => "ice",

            // Computed precip type: sleet
            () => "mix"
        );
    }

    public string GetTempFeel(Data meteoData)
    {
        float num = meteoData.T_SH;
        float num2 = meteoData.T_SL;
        float num3 = meteoData.T_NH;

        float colder = _meteoScalesService.Temperature.Colder;
        float cold = _meteoScalesService.Temperature.Cold;
        float warm = _meteoScalesService.Temperature.Warm;
        float warmer = _meteoScalesService.Temperature.Warmer;
        float hot = _meteoScalesService.Temperature.Hot;
        float frost = _meteoScalesService.Temperature.Frost;

        if (num >= hot)
            return "hot";

        if (num2 <= frost || num <= frost)
            return "frost";

        if (num > num3 + warmer)
            return "much_warmer";

        if (num > num3 + warm)
            return "warmer";

        if (num < num3 + colder)
            return "much_colder";

        if (num < num3 + cold)
            return "colder";

        return "normal";
    }
}

