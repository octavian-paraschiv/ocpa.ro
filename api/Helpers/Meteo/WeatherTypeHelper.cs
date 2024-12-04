using System;
using System.Collections.Generic;
using ThorusCommon.IO;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Helpers.Meteo
{
    public static class WeatherTypeHelper
    {
        static readonly string[] wdirs =
        [
                "W", "WSW", "SW", "SSW", "S", "SSE", "SE", "ESE", "E", "ENE", "NE", "NNE", "N", "NNW", "NW", "WNW"
        ];


        public static string GetWeatherType(MeteoScaleHelpers scale, Data meteoData, int wind, List<string> risks)
        {
            float precip = meteoData.C_00;
            float inst = -meteoData.L_00;
            float fog = meteoData.F_SI;

            var inst_threshold = scale.Instability.Weak;
            var inst_heavy = scale.Instability.Heavy;

            var precip_weak = scale.Precip.Weak;
            var precip_moderate = scale.Precip.Moderate;
            var precip_heavy = scale.Precip.Heavy;
            var precip_extreme = scale.Precip.Extreme;

            var fog_weak = scale.Fog.Weak;
            var fog_moderate = scale.Fog.Moderate;
            var fog_heavy = scale.Fog.Heavy;
            var fog_extreme = scale.Fog.Extreme;

            var wind_weak = scale.Wind.Weak;
            var wind_moderate = scale.Wind.Moderate;
            var wind_heavy = scale.Wind.Heavy;
            var wind_extreme = scale.Wind.Extreme;

            string intensity = "00";
            string type = GetPrecipType(scale, meteoData);
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

            if (wind >= wind_extreme)
                risks.Add("heavy_wind");
            else if (wind >= wind_heavy)
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

        public static int GetWind(Data meteoData, out string direction)
        {
            var w00 = meteoData.W_00;
            var w01 = meteoData.W_01;
            var w10 = meteoData.W_10;
            var w11 = meteoData.W_11;

            var slice = Math.Floor(4 * (w10 + w11) / Math.PI);
            var idx = (int)Math.Min(wdirs.Length - 1, Math.Max(0, slice));
            direction = wdirs[idx];

            return (int)Math.Round(7.6f * (w00 + w01));
        }

        public static string GetPrecipType(MeteoScaleHelpers scale, Data meteoData)
        {
            var te = meteoData.T_TE;
            var ts = meteoData.T_TS;
            var t01 = meteoData.T_01;

            return PrecipTypeComputer.Compute<string>(
                // Actual temperatures
                te, ts, t01,

                // Boundary temperatures as read from config file
                scale.Boundaries,

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

        public static string GetTempFeel(MeteoScaleHelpers scale, Data meteoData)
        {
            float num = meteoData.T_SH;
            float num2 = meteoData.T_SL;
            float num3 = meteoData.T_NH;

            float colder = scale.Temperature.Colder;
            float cold = scale.Temperature.Cold;
            float warm = scale.Temperature.Warm;
            float warmer = scale.Temperature.Warmer;
            float hot = scale.Temperature.Hot;
            float frost = scale.Temperature.Frost;

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

