using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meteo.Helpers
{
    public static class StringHelper
    {
        public static string GetToolTip(string dtStr, string precip, string tempType, bool windy)
        {
            string windSuffix = string.Empty;
            if (windy)
                windSuffix += "; Windy";

            string weatherType = "";
            switch (precip)
            {
                case "00":
                    weatherType = "Mostly sunny" + windSuffix;
                    break;

                case "01_rain":
                    weatherType = "Partly sunny with rain showers" + windSuffix;
                    break;
                case "01_inst":
                    weatherType = "Partly sunny with thunder showers" + windSuffix;
                    break;
                case "01_snow":
                    weatherType = "Partly sunny with snow showers" + windSuffix;
                    break;
                case "01_mix":
                    weatherType = "Partly sunny with sleet showers" + windSuffix;
                    break;
                case "01_ice":
                    weatherType = "Partly sunny with freezing rain showers" + windSuffix;
                    break;

                case "02_rain":
                    weatherType = "Rain" + windSuffix;
                    break;
                case "02_inst":
                    weatherType = "Rain with thunder" + windSuffix;
                    break;
                case "02_snow":
                    weatherType = "Snow" + windSuffix;
                    break;
                case "02_mix":
                    weatherType = "Sleet" + windSuffix;
                    break;
                case "02_ice":
                    weatherType = "Freezing rain" + windSuffix;
                    break;

                case "03_rain":
                    weatherType = "Intense rain" + windSuffix;
                    break;
                case "03_inst":
                    weatherType = "Thunderstorms" + windSuffix;
                    break;
                case "03_snow":
                    weatherType = "Intense snow" + windSuffix;
                    break;
                case "03_mix":
                    weatherType = "Intense sleet" + windSuffix;
                    break;
                case "03_ice":
                    weatherType = "Intense freezing rain" + windSuffix;
                    break;

                case "04_rain":
                    weatherType = "Heavy rain" + windSuffix;
                    break;
                case "04_inst":
                    weatherType = "Heavy thunderstorms" + windSuffix;
                    break;
                case "04_snow":
                    weatherType = "Heavy snow" + windSuffix;
                    break;
                case "04_mix":
                    weatherType = "Heavy sleet" + windSuffix;
                    break;
                case "04_ice":
                    weatherType = "Heavy freezing rain" + windSuffix;
                    break;

                case "01_fog":
                    weatherType = "Some morning fog" + windSuffix;
                    break;
                case "02_fog":
                    weatherType = "Fog dissipates until noon" + windSuffix;
                    break;
                case "03_fog":
                    weatherType = "Persistent fog" + windSuffix;
                    break;
                case "04_fog":
                    weatherType = "Very dense fog" + windSuffix;
                    break;

                case "01_wind":
                    weatherType = "Mostly sunny with some wind";
                    break;
                case "02_wind":
                    weatherType = "Partly sunny with moderate wind";
                    break;
                case "03_wind":
                    weatherType = "Strong wind";
                    break;
                case "04_wind":
                    weatherType = "Heavy wind";
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