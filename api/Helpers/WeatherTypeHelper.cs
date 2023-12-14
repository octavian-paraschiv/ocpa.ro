namespace ocpa.ro.api.Helpers
{
    using System;
    using System.Collections.Generic;
    using ThorusCommon.IO;
    using ThorusCommon.SQLite;

    namespace Meteo.Helpers
    {
        public class WeatherTypeHelper
        {
            IMeteoDataHelper _dataHelper;

            static readonly string[] wdirs = new string[]
            {
                "W", "WSW", "SW", "SSW", "S", "SSE", "SE", "ESE", "E", "ENE", "NE", "NNE", "N", "NNW", "NW", "WNW"
            };

            public WeatherTypeHelper(IMeteoDataHelper meteoDataHelper)
            {
                _dataHelper = meteoDataHelper;
            }

            public string GetWeatherType(Data meteoData, List<String> risks)
            {
                float precip = meteoData.C_00;
                float inst = -meteoData.L_00;
                float fog = meteoData.F_SI;

                float wind = GetWind(meteoData, out string _);

                var inst_threshold = _dataHelper.Scale.Instability.Weak;
                var inst_heavy = _dataHelper.Scale.Instability.Heavy;

                var precip_weak = _dataHelper.Scale.Precip.Weak;
                var precip_moderate = _dataHelper.Scale.Precip.Moderate;
                var precip_heavy = _dataHelper.Scale.Precip.Heavy;
                var precip_extreme = _dataHelper.Scale.Precip.Extreme;

                var fog_weak = _dataHelper.Scale.Fog.Weak;
                var fog_moderate = _dataHelper.Scale.Fog.Moderate;
                var fog_heavy = _dataHelper.Scale.Fog.Heavy;
                var fog_extreme = _dataHelper.Scale.Fog.Extreme;

                var wind_weak = _dataHelper.Scale.Wind.Weak;
                var wind_moderate = _dataHelper.Scale.Wind.Moderate;
                var wind_heavy = _dataHelper.Scale.Wind.Heavy;
                var wind_extreme = _dataHelper.Scale.Wind.Extreme;

                string intensity = "00";
                string type = GetPrecipType(meteoData);
                string actualPrecipType = "";

                if (type == "rain" && inst >= inst_threshold)
                    type = "inst";

                actualPrecipType = type;
                if (type == "inst" || type == "ice")
                    actualPrecipType = "rain";
                if (type == "mix")
                    actualPrecipType = "rain/snow";

                if (precip >= precip_extreme)
                {
                    intensity = "04";
                    risks.Add($"Heavy {actualPrecipType}");
                }
                else if (precip >= precip_heavy)
                {
                    intensity = "03";
                    risks.Add($"Intense {actualPrecipType}");
                }
                else if (precip >= precip_moderate)
                    intensity = "02";
                else if (precip >= precip_weak)
                    intensity = "01";

                if (fog <= fog_extreme)
                    risks.Add("Very dense fog");
                else if (fog <= fog_heavy)
                    risks.Add("Persistent fog");

                if (wind >= wind_extreme)
                    risks.Add("Heavy wind");
                else if (wind >= wind_heavy)
                    risks.Add("Strong wind");

                if (intensity != "00")
                {
                    if (inst >= inst_heavy)
                        risks.Add("Squalls and hail");

                    if (type == "ice")
                        risks.Add("Freezing rain");

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

                if (wind >= wind_extreme)
                    return "04_wind";
                else if (wind >= wind_heavy)
                    return "03_wind";
                else if (wind >= wind_moderate)
                    return "02_wind";
                else if (wind >= wind_weak)
                    return "01_wind";

                return "00";
            }

            public int GetWind(Data meteoData, out string direction)
            {
                var w00 = meteoData.W_00;
                var w01 = meteoData.W_01;
                var w10 = meteoData.W_10;
                var w11 = meteoData.W_11;

                var slice = Math.Floor(4 * (w10 + w11) / Math.PI);
                var idx = (int)Math.Min(wdirs.Length - 1, Math.Max(0, slice));
                direction = wdirs[idx];

                return (int)Math.Floor(7.6f * (w00 + w01));
            }

            public string GetPrecipType(Data meteoData)
            {
                var te = meteoData.T_TE;
                var ts = meteoData.T_TS;
                var t01 = meteoData.T_01;

                return PrecipTypeComputer<string>.Compute(
                    // Actual temperatures
                    te, ts, t01,

                    // Boundary temperatures as read from config file
                    _dataHelper.Scale.Boundaries,

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
                float num4 = meteoData.T_NL;

                float colder = _dataHelper.Scale.Temperature.Colder;
                float cold = _dataHelper.Scale.Temperature.Cold;
                float warm = _dataHelper.Scale.Temperature.Warm;
                float warmer = _dataHelper.Scale.Temperature.Warmer;
                float hot = _dataHelper.Scale.Temperature.Hot;
                float frost = _dataHelper.Scale.Temperature.Frost;

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
    }
}
