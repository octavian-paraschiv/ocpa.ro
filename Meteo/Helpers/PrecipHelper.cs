﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meteo.Helpers
{
    public static class PrecipHelper
    {
        public static string GetWeatherType(DateTime dt, int r, int c, List<String> risks)
        {
            float precip = DataHelper.GetDataPoint("C_00", dt, r, c);
            float inst = -1 * DataHelper.GetDataPoint("L_00", dt, r, c);

            float fog = DataHelper.GetDataPoint("F_SI", dt, r, c);

            var inst_threshold = ScaleSettings.Instability.Weak;
            var inst_heavy = ScaleSettings.Instability.Heavy;

            var precip_weak = ScaleSettings.Precip.Weak;
            var precip_moderate = ScaleSettings.Precip.Moderate;
            var precip_heavy = ScaleSettings.Precip.Heavy;
            var precip_extreme = ScaleSettings.Precip.Extreme;

            var fog_weak = ScaleSettings.Fog.Weak;
            var fog_moderate = ScaleSettings.Fog.Moderate;
            var fog_heavy = ScaleSettings.Fog.Heavy;
            var fog_extreme = ScaleSettings.Fog.Extreme;

            string intensity = "00";
            string type = GetPrecipType(dt, r, c);
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

            if (intensity != "00")
            {
                if (inst >= inst_heavy)
                    risks.Add("Squalls and hail");

                if (type == "ice")
                    risks.Add("Freezing rain");

                return $"{intensity}_{type}";
            }

            if (fog <= fog_extreme)
            {
                risks.Add("Very dense fog");
                return "04_fog";
            }
            else if (fog <= fog_heavy)
            {
                risks.Add("Persistent fog");
                return "03_fog";
            }
            else if (fog <= fog_moderate)
            {
                return "02_fog";
            }
            else if (fog <= fog_weak)
            {
                return "01_fog";
            }

            return "00";
        }

        public static int GetSnowThickness(DateTime dt, int r, int c)
        {
            var snow = DataHelper.GetDataPoint("N_00", dt, r, c);
            return (int)Math.Floor(snow);
        }

        public static string GetPrecipType(DateTime dt, int r, int c)
        {
            int tMin = (int)DataHelper.GetDataPoint("T_SL", dt, r, c);
            int tMax = (int)DataHelper.GetDataPoint("T_SH", dt, r, c);
            int te = (int)DataHelper.GetDataPoint("T_TE", dt, r, c);
            int ts = (int)DataHelper.GetDataPoint("T_TS", dt, r, c);
            int t01 = (int)DataHelper.GetDataPoint("T_01", dt, r, c);

            var MaxFreezingRainDelta = ScaleSettings.Boundaries.MaxFreezingRainDelta;
            var MaxTeForSolidPrecip = ScaleSettings.Boundaries.MaxTeForSolidPrecip;
            var MaxTsForFreezing = ScaleSettings.Boundaries.MaxTsForFreezing;
            var MinTeForLiquidPrecip = ScaleSettings.Boundaries.MinTeForLiquidPrecip;
            var MinTsForMelting = ScaleSettings.Boundaries.MinTsForMelting;

            if (te < MaxTeForSolidPrecip)
                // Snow (for sure)
                return "snow";

            if (te > MinTeForLiquidPrecip)
            {
                if (ts > MinTsForMelting)
                    // Rain (for sure)
                    return "rain";
                else
                {
                    // Freezing rain conditions as the surface is below 0.
                    if (ts <= MaxTsForFreezing && (te - t01) < MaxFreezingRainDelta)
                        return "ice";
                }
            }
            else
            {
                if (ts > MinTsForMelting)
                    // Sleet (for sure).
                    return "mix";

                // But: If the surface is below 0 then we again have freezing rain.
                else if ((te - t01) < MaxFreezingRainDelta)
                    return "ice";

                // Otherwise sleet.
                return "mix";
            }

            return "rain";
        }
    }
}