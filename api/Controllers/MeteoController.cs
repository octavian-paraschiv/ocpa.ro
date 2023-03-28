using api.Controllers.Models;
using api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ocpa.ro.api.Helpers;
using ocpa.ro.api.Helpers.Meteo.Helpers;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;
using ThorusCommon.IO;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MeteoController : ApiControllerBase
	{
		static MeteoScaleSettings _scale;
		static IniFile _iniFile;

		private MeteoDataHelper GetDataHelper()
		{
			string dataFolder = System.IO.Path.Combine(ContentPath, $"meteo");
			return new MeteoDataHelper(dataFolder, _scale);
		}


		public MeteoController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper) 
			: base(hostingEnvironment, authHelper)
		{
			Init();
		}

		private void Init()
		{
			if (_scale != null)
				return;

			var iniPath = System.IO.Path.Combine(ContentPath, $"meteo/ScaleSettings.ini");
			_iniFile = new IniFile(iniPath);
			_scale = new MeteoScaleSettings(_iniFile);
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
				GetDataHelper().HandleDatabasePart(part);
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
			return GetDataHelper().GetCalendarRange(0);
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
				var dataHelper = GetDataHelper();

				meteoData.CalendarRange = new CalendarRange
				{
					Start = range.Start.AddDays(skip)
				};

				skip = Math.Min(range.Length - 1, Math.Max(0, skip));

				int remaining = range.Length - skip;

				take = Math.Min(range.Length - skip, Math.Max(0, range.Length));

				var allData = dataHelper.GetData(region, gc, skip, take);
				if (allData?.Count > 0)
				{
					var weatherHelper = new WeatherTypeHelper(dataHelper);

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
