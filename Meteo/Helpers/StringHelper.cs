using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meteo.Helpers
{
    public static class StringHelper
    {
        public static string GetToolTip(string dtStr, string precip, string tempType)
        {
            string weatherType = "";
            switch (precip)
            {
                case "00":
                    weatherType = "Mostly sunny";
                    break;

                case "01_rain":
                    weatherType = "Partly sunny with rain showers";
                    break;
                case "01_inst":
                    weatherType = "Partly sunny with thunder showers";
                    break;
                case "01_snow":
                    weatherType = "Partly sunny with snow showers";
                    break;
                case "01_mix":
                    weatherType = "Partly sunny with sleet showers";
                    break;
                case "01_ice":
                    weatherType = "Partly sunny with freezing rain showers";
                    break;

                case "02_rain":
                    weatherType = "Rain";
                    break;
                case "02_inst":
                    weatherType = "Rain with thunder";
                    break;
                case "02_snow":
                    weatherType = "Snow";
                    break;
                case "02_mix":
                    weatherType = "Sleet";
                    break;
                case "02_ice":
                    weatherType = "Freezing rain";
                    break;

                case "03_rain":
                    weatherType = "Intense rain";
                    break;
                case "03_inst":
                    weatherType = "Thunderstorms";
                    break;
                case "03_snow":
                    weatherType = "Intense snow";
                    break;
                case "03_mix":
                    weatherType = "Intense sleet";
                    break;
                case "03_ice":
                    weatherType = "Intense freezing rain";
                    break;

                case "04_rain":
                    weatherType = "Heavy rain";
                    break;
                case "04_inst":
                    weatherType = "Heavy thunderstorms";
                    break;
                case "04_snow":
                    weatherType = "Heavy snow";
                    break;
                case "04_mix":
                    weatherType = "Heavy sleet";
                    break;
                case "04_ice":
                    weatherType = "Heavy freezing rain";
                    break;

                case "01_fog":
                    weatherType = "Some morning fog";
                    break;
                case "02_fog":
                    weatherType = "Fog dissipates until noon";
                    break;
                case "03_fog":
                    weatherType = "Persistent fog";
                    break;
                case "04_fog":
                    weatherType = "Very dense fog";
                    break;
            }

            if (string.IsNullOrEmpty(weatherType))
                weatherType = "Overcast";

            if (string.IsNullOrEmpty(tempType))
                tempType = "Seasonable temperatures";

            return $"{dtStr}\r\n{weatherType}\r\n{tempType}";
        }

        public static string Scale(int temp)
        {
            bool useC = true;

            if (useC)
                return $"{temp}&#x2103;";

            return $"{(int)Math.Round(9 * (temp + 32f) / 5)}&#x2109;";
        }
    }
}