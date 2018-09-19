using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meteo.Helpers
{
    public static class RefTemp
    {
        public static readonly int HoursPerDay = 24;
        public static readonly float DefaultRefTemp = 25;

        public static void GetRefTemps_ByLatitude(int dayOfyear, float lat, ref float tMin, ref float tMax)
        {
            // Reference temp at 1500 m
            float refTemp = GetRefTemp_ByLatitude(dayOfyear, lat);
            float te = refTemp + 10;

            float dl = GetDayLength_ByLatitude(dayOfyear, lat);
            float sunLatRad = GetSunLatitude_Radians(dayOfyear);

            float sunAngle = (float)Math.PI * lat / 180 - sunLatRad;
            float sf = (float)Math.Cos(sunAngle);

            tMax = te + dl * sf;
            tMin = 2 * te - tMax;
        }

        public static float GetRefTemp_ByLatitude(int dayOfyear, float lat)
        {
            float init = 0;

            float latRad = lat * (float)Math.PI / 180;

            float sunLatRad = GetSunLatitude_Radians(dayOfyear);

            float sunLat = sunLatRad * 180 / (float)Math.PI;

            float deltaLatRad = Math.Abs(latRad - sunLatRad);
            float deltaLat = Math.Abs(lat - sunLat);

            float dayLen = GetDayLength_ByLatitude(dayOfyear, lat);
            float tm = DefaultRefTemp;

            init = 25 - 0.5f * deltaLat;

            float absCosLat = (float)Math.Abs(Math.Cos(deltaLatRad));

            float deltaByNight = 4f * absCosLat * (dayLen / HoursPerDay - 0.5f);

            float te = init + deltaByNight;

            return te;
        }


        public static float GetDayLength_ByLatitude(int dayOfYear, float lat)
        {
            float dayLen = 0;

            float latRan = lat * (float)Math.PI / 180;
            float p = GetSunLatitude_Radians(dayOfYear);

            float sin1 = (float)Math.Sin(0.8333f * (float)Math.PI / 180);
            float prod_sin = (float)Math.Sin(latRan) * (float)Math.Sin(p);
            float prod_cos = (float)Math.Cos(latRan) * (float)Math.Cos(p);

            float arg_acos = (sin1 + prod_sin) / prod_cos;
            if (arg_acos < -1)
                arg_acos = -1;
            if (arg_acos > 1)
                arg_acos = 1;

            float acos_val = (float)Math.Acos(arg_acos);

            float nightLen = HoursPerDay / (float)Math.PI * acos_val;

            if (nightLen > HoursPerDay)
                dayLen = 0;
            else if (nightLen < 0)
                dayLen = HoursPerDay;
            else
                dayLen = HoursPerDay - nightLen;

            return dayLen;
        }

        public static float GetSunLatitude_Radians(int dayOfYear)
        {
            dayOfYear = (dayOfYear + (int)ScaleSettings.Temperature.Delay);

            float p = (float)Math.Asin(0.39795f * (float)Math.Cos(0.2163108f + 2 * (float)Math.Atan(0.9671396f * (float)Math.Tan(0.00860f * (dayOfYear - 182.625f)))));
            return p;
        }


    }
}