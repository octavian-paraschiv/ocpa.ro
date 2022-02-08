using common;
using Meteo.Helpers;
using OPMedia.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using ThorusCommon.IO;

namespace OPMedia.API.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class MeteoController : ApiController
    {
        public CalendarRange GetRange(string region)
        {
            string dataPath = HostingEnvironment.MapPath($"/meteo/data/submatrix_{region}");
            return new MeteoDataHelper(dataPath).GetCalendarRange(0);
        }

        public MeteoData GetMeteoData(string region, string subregion, string city, int skip, int take = 0)
        {
            var gc = new GeographyController().GetGridCoordinates(region, subregion, city);
            return GetMeteoData(gc, region, skip, take);
        }

        private MeteoData GetMeteoData(GridCoordinates gc, string region, int skip, int take = 0)
        {
            MeteoData md = new MeteoData { GridCoordinates = gc };

            try
            {
                if (take <= 0)
                    take = 1;

                var fullRange = GetRange(region);

                md.CalendarRange = new CalendarRange
                {
                    Start = fullRange.Start.AddDays(skip),
                };

                if (fullRange.Length > skip)
                {
                    int dataSize = Math.Min(take, fullRange.Length - skip);

                    md.Data = new Dictionary<string, MeteoDailyData>();
                    md.CalendarRange.End = fullRange.Start.AddDays(skip + dataSize - 1);

                    for (int i = skip; i < skip + dataSize; i++)
                    {
                        DateTime dt = fullRange.Start.AddDays(i);
                        var map = GetMeteoData(dt, gc, region);

                        MeteoDailyData data = new MeteoDailyData
                        {
                            TMaxActual = Math.Round(map["T_SH"], 0).ToString(),
                            TMinActual = Math.Round(map["T_SL"], 0).ToString(),
                            TMaxNormal = Math.Round(map["T_NH"], 0).ToString(),
                            TMinNormal = Math.Round(map["T_NL"], 0).ToString(),
                            Forecast = GetForecast(map),
                            TempFeel = GetTempFeel(map)
                        };

                        md.Data.Add(dt.ToString(MeteoConstants.DateFormat), data);
                    }

                    md.CalendarRange.Length = dataSize;
                }
            }
            catch { }

            return md;
        }

        Dictionary<string, float> GetMeteoData(DateTime dt, GridCoordinates gc, string region)
        {
            Dictionary<string, float> map = new Dictionary<string, float>();

            try
            {
                string dataPath = HostingEnvironment.MapPath($"/meteo/data/submatrix_{region}");
                var helper = new MeteoDataHelper(dataPath);
                
                var types = helper.GetDataTypes(dt);

                foreach (string type in types)
                {
                    var pt = helper.GetDataPoint(type, dt, gc);
                    map.Add(type, pt);
                }
            }
            catch
            {
            }

            return map;
        }

        static string GetForecast(Dictionary<string, float> data)
        {
            var precip =    data["C_00"];
            
            var te =        data["T_TE"];
            var ts =        data["T_TS"];
            var t01 =       data["T_01"];

            var inst = -1 * data["L_00"];
            var fog =       data["F_SI"];
            var wind =      GetWind(data);

            var precipType = PrecipTypeComputer<string>.Compute(
                // Actual temperatures
                te, ts, t01,

                // Boundary temperatures as read from config file
                MeteoScaleSettings.Boundaries,

                // Computed precip type: snow
                () => "snow",

                // Computed precip type: rain
                () => (inst >= MeteoScaleSettings.Instability.Weak ? "inst" : "rain"),

                // Computed precip type: freezing rain
                () => "ice",

                // Computed precip type: sleet
                () => "mix"
            );

            string intensity = "00";

            if (precip >= MeteoScaleSettings.Precip.Extreme)
            {
                intensity = $"04_{precipType}";
            }
            else if (precip >= MeteoScaleSettings.Precip.Heavy)
            {
                intensity = $"03_{precipType}";
            }
            else if (precip >= MeteoScaleSettings.Precip.Moderate)
            {
                intensity = $"02_{precipType}";
            }
            else if (precip >= MeteoScaleSettings.Precip.Weak)
            {
                intensity = $"01_{precipType}";
            }
            else
            {
                if (fog <= MeteoScaleSettings.Fog.Extreme)
                    return "04_fog";
                else if (fog <= MeteoScaleSettings.Fog.Heavy)
                    return "03_fog";
                else if (fog <= MeteoScaleSettings.Fog.Moderate)
                    return "02_fog";
                else if (fog <= MeteoScaleSettings.Fog.Weak)
                    return "01_fog";

                if (wind >= MeteoScaleSettings.Wind.Extreme)
                    return "04_wind";
                else if (wind >= MeteoScaleSettings.Wind.Heavy)
                    return "03_wind";
                else if (wind >= MeteoScaleSettings.Wind.Moderate)
                    return "02_wind";
                else if (wind >= MeteoScaleSettings.Wind.Weak)
                    return "01_wind";
            }

            return intensity;
        }

        private string GetTempFeel(Dictionary<string, float> map)
        {
            var tMax = map["T_SH"];
            var tMin = map["T_SL"];
            var tRefMax = map["T_NH"];
            var tRefMin = map["T_NL"];

            var colder = MeteoScaleSettings.Temperature.Colder;
            var cold = MeteoScaleSettings.Temperature.Cold;
            var warm = MeteoScaleSettings.Temperature.Warm;
            var warmer = MeteoScaleSettings.Temperature.Warmer;
            var hot = MeteoScaleSettings.Temperature.Hot;
            var frost = MeteoScaleSettings.Temperature.Frost;

            if (tMax >= hot)
                return "hot";
            if (tMin <= frost || tMax <= frost)
                return "frost";
            if (tMax > (tRefMax + warmer))
                return "much_warmer";
            if (tMax > (tRefMax + warm))
                return "warmer";
            if (tMax < (tRefMax + colder))
                return "much_colder";
            if (tMax < (tRefMax + cold))
                return "colder";

            return "normal";
        }


        static float GetWind(Dictionary<string, float> data)
        {
            var w00 = data["W_00"];
            var w01 = data["W_01"];
            var w = 0.5f * (w00 + w01);

            if (w <= 2)
                return 0;

            return 6f * w;
        }
    }
}
