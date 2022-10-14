using api.Controllers.Models;
using api.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;
using ThorusCommon.IO;

namespace ocpa.ro.api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MeteoController : ApiControllerBase
	{
		static MeteoScaleSettings _scale;
		static IniFile _iniFile;

		public MeteoController(IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
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

        [HttpGet("range")]
		public IActionResult GetRange([FromQuery] string region)
		{
			try
			{
				_ = new GeographyController(_hostingEnvironment).GetRegion(region);
				var range = _GetRange(region);
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

		private CalendarRange _GetRange(string region)
		{
			string dataFolder = System.IO.Path.Combine(ContentPath, $"meteo/submatrix_{region}");
			return new MeteoDataHelper(dataFolder).GetCalendarRange(0);
		}

		private MeteoData GetMeteoData(GridCoordinates gc, string region, int skip, int take)
		{
			MeteoData meteoData = new MeteoData
			{
				GridCoordinates = gc
			};
			try
			{
				CalendarRange range = _GetRange(region);
				meteoData.CalendarRange = new CalendarRange
				{
					Start = range.Start.AddDays(skip)
				};

				if (take <= 0)
				{
					take = range.Length;
				}

				if (range.Length > skip)
				{
					int num = Math.Min(take, range.Length - skip);
					meteoData.Data = new Dictionary<string, MeteoDailyData>();
					meteoData.CalendarRange.End = range.Start.AddDays(skip + num - 1);
					for (int i = skip; i < skip + num; i++)
					{
						DateTime dt = range.Start.AddDays(i);
						Dictionary<string, float> meteoData2 = GetMeteoData(dt, gc, region);
						MeteoDailyData value = new MeteoDailyData
						{
							TMaxActual = (int)Math.Round(meteoData2["T_SH"], 0),
							TMinActual = (int)Math.Round(meteoData2["T_SL"], 0),
							TMaxNormal = (int)Math.Round(meteoData2["T_NH"], 0),
							TMinNormal = (int)Math.Round(meteoData2["T_NL"], 0),
							Forecast = GetForecast(meteoData2),
							TempFeel = GetTempFeel(meteoData2)
						};
						meteoData.Data.Add(dt.ToString("yyyy-MM-dd"), value);
					}
					meteoData.CalendarRange.Length = num;
				}
			}
			catch
			{
			}
			return meteoData;
		}

		private Dictionary<string, float> GetMeteoData(DateTime dt, GridCoordinates gc, string region)
		{
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			try
			{
				string dataFolder = System.IO.Path.Combine(ContentPath, $"meteo/submatrix_{region}");
				MeteoDataHelper meteoDataHelper = new MeteoDataHelper(dataFolder);
				List<string> dataTypes = meteoDataHelper.GetDataTypes(dt);
				foreach (string item in dataTypes)
				{
					float dataPoint = meteoDataHelper.GetDataPoint(item, dt, gc);
					dictionary.Add(item, dataPoint);
				}
			}
			catch
			{
			}
			return dictionary;
		}

		private static string GetForecast(Dictionary<string, float> data)
		{
			float num = data["C_00"];
			float te = data["T_TE"];
			float ts = data["T_TS"];
			float t = data["T_01"];
			float inst = -1f * data["L_00"];
			float num2 = data["F_SI"];
			float wind = GetWind(data);

			string text = PrecipTypeComputer<string>.Compute(te, ts, t,
				_scale.Boundaries, 
				() => "snow", 
				() => (inst >= _scale.Instability.Weak) ? "inst" : "rain", 
				() => "ice", 
				() => "mix"
			);

			string result = "00";
			if (num >= _scale.Precip.Extreme)
			{
				result = "04_" + text;
			}
			else if (num >= _scale.Precip.Heavy)
			{
				result = "03_" + text;
			}
			else if (num >= _scale.Precip.Moderate)
			{
				result = "02_" + text;
			}
			else if (num >= _scale.Precip.Weak)
			{
				result = "01_" + text;
			}
			else
			{
				if (num2 <= _scale.Fog.Extreme)
				{
					return "04_fog";
				}
				if (num2 <= _scale.Fog.Heavy)
				{
					return "03_fog";
				}
				if (num2 <= _scale.Fog.Moderate)
				{
					return "02_fog";
				}
				if (num2 <= _scale.Fog.Weak)
				{
					return "01_fog";
				}
				if (wind >= _scale.Wind.Extreme)
				{
					return "04_wind";
				}
				if (wind >= _scale.Wind.Heavy)
				{
					return "03_wind";
				}
				if (wind >= _scale.Wind.Moderate)
				{
					return "02_wind";
				}
				if (wind >= _scale.Wind.Weak)
				{
					return "01_wind";
				}
			}
			return result;
		}

		private string GetTempFeel(Dictionary<string, float> map)
		{
			float num = map["T_SH"];
			float num2 = map["T_SL"];
			float num3 = map["T_NH"];
			float num4 = map["T_NL"];

			float colder = _scale.Temperature.Colder;
			float cold = _scale.Temperature.Cold;
			float warm = _scale.Temperature.Warm;
			float warmer = _scale.Temperature.Warmer;
			float hot = _scale.Temperature.Hot;
			float frost = _scale.Temperature.Frost;

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

		private static float GetWind(Dictionary<string, float> data)
		{
			float num = data["W_00"];
			float num2 = data["W_01"];
			float num3 = 0.5f * (num + num2);
			if (num3 <= 2f)
			{
				return 0f;
			}
			return 6f * num3;
		}
	}
}
