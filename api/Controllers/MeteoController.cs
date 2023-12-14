using api.Controllers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ocpa.ro.api.Helpers;
using ocpa.ro.api.Helpers.Meteo.Helpers;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MeteoController : ApiControllerBase
    {
        private IMeteoDataHelper _dataHelper = null;

        public MeteoController(IWebHostEnvironment hostingEnvironment, IMeteoDataHelper meteoDataHelper)
            : base(hostingEnvironment)
        {
            _dataHelper = meteoDataHelper;
        }


        const long MaxFileSize = 16 * 1024L * 1024L;

        [Authorize]
        [HttpPost("uploadPart")]
        [RequestSizeLimit(MaxFileSize)]
        public IActionResult UploadPart([FromBody] UploadDataPart part)
        {
            try
            {
                var body = JsonConvert.SerializeObject(part);
                _dataHelper.HandleDatabasePart(part);
                return Ok();
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized(uae.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("dataFolder")]
        public IActionResult GetContentPath()
        {
            return Ok(ContentPath);
        }

        [HttpGet("range")]
        public IActionResult GetRange()
        {
            try
            {
                var range = _GetRange();
                return Ok(range);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("data")]
        public IActionResult GetMeteoData([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = new GeographyController(_hostingEnvironment)._GetGridCoordinates(region, subregion, city);
                var data = GetMeteoData(gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private CalendarRange _GetRange()
        {
            return _dataHelper.GetCalendarRange(0);
        }

        private MeteoData GetMeteoData(GridCoordinates gc, string region, int skip, int take)
        {
            MeteoData meteoData = new MeteoData
            {
                GridCoordinates = gc
            };

            try
            {
                CalendarRange range = _GetRange();

                meteoData.CalendarRange = new CalendarRange
                {
                    Start = range.Start.AddDays(skip)
                };

                skip = Math.Min(range.Length - 1, Math.Max(0, skip));

                int remaining = range.Length - skip;

                take = Math.Min(range.Length - skip, Math.Max(0, range.Length));

                var allData = _dataHelper.GetData(region, gc, skip, take);
                if (allData?.Count > 0)
                {
                    var weatherHelper = new WeatherTypeHelper(_dataHelper);

                    meteoData.Data = new Dictionary<string, MeteoDailyData>();
                    meteoData.CalendarRange.End = range.Start.AddDays(allData.Count - 1);
                    meteoData.CalendarRange.Length = allData.Count;

                    allData.ForEach(d =>
                    {
                        List<string> risks = new List<string>();
                        var forecast = weatherHelper.GetWeatherType(d, risks);
                        var wind = weatherHelper.GetWind(d, out string direction);

                        MeteoDailyData value = new MeteoDailyData
                        {
                            TMaxActual = d.T_SH.Round(),
                            TMinActual = d.T_SL.Round(),
                            TMaxNormal = d.T_NH.Round(),
                            TMinNormal = d.T_NL.Round(),
                            SnowCover = d.N_00.Round(),
                            Precip = d.C_00.Round(),
                            Instability = d.L_00.Round(),
                            Fog = -d.F_SI.Round() + 100,
                            SoilRain = d.R_00.Round(),
                            Rain = d.R_DD.Round(),
                            Snow = d.N_DD.Round(),
                            P00 = d.P_00.Round(),
                            P01 = d.P_01.Round(),

                            TempFeel = weatherHelper.GetTempFeel(d),
                            Wind = wind,
                            WindDirection = direction,

                            Hazards = risks,
                            Forecast = forecast,
                        };

                        meteoData.Data.Add(d.Timestamp, value);
                    });
                }
            }
            catch
            {
            }
            return meteoData;
        }
    }
}
