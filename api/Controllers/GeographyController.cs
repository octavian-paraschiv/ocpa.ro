
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ocpa.ro.api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class GeographyController : ApiControllerBase
	{
		private static string _configFilePath;
		private static FileSystemWatcher _watchConfigFile;
		private static List<Region> _regions;

		public GeographyController(IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
		{
			Init();
		}

		private void Init()
		{
			if (_regions?.Count > 0)
				return;

			_configFilePath = null;
			_watchConfigFile = null;
			_regions = new List<Region>();
			_configFilePath = Path.Combine(ContentPath, $"meteo/GeographicData.json");

			ReadConfigFile();
			_watchConfigFile = new FileSystemWatcher
			{
				Path = Path.GetDirectoryName(_configFilePath),
				Filter = Path.GetFileName(_configFilePath),
				EnableRaisingEvents = true
			};
			_watchConfigFile.Changed += delegate (object s, FileSystemEventArgs e)
			{
				try
				{
					WatcherChangeTypes changeType = e.ChangeType;
					WatcherChangeTypes watcherChangeTypes = changeType;
					if (watcherChangeTypes == WatcherChangeTypes.Created || watcherChangeTypes == WatcherChangeTypes.Changed)
					{
						ReadConfigFile();
					}
				}
				catch
				{
				}
			};
		}

		private static void ReadConfigFile()
		{
			string value = System.IO.File.ReadAllText(_configFilePath);
			_regions = JsonConvert.DeserializeObject<List<Region>>(value);
		}

        [HttpGet("regions")]
		public IActionResult GetRegionNames()
		{
			try
			{
				var query = from rgn in _regions
							orderby rgn.Name
							select rgn.Name;

				return Ok(query.Distinct().ToList());
			}
			catch (Exception ex)
            {
				return BadRequest(ex.Message);
            }
		}

		[HttpGet("subregions")]
		public IActionResult GetSubregionNames([FromQuery] string region)
		{
			try
			{
				_ = GetRegion(region);

				var query = from rgn in _regions
							from city in rgn.Cities
							
							where 
								string.Equals(rgn.Name, region, StringComparison.OrdinalIgnoreCase)

							orderby city.Subregion
							select city.Subregion;

				return Ok(query.Distinct().ToList());
			}
			catch (Exception ex)
            {
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("cities")]
		public IActionResult GetCityNames([FromQuery] string region, [FromQuery] string subregion)
		{
			try
			{
				ValidateSubregion(region, subregion);

				var query = from rgn in _regions
							from city in rgn.Cities
							
							where 
								string.Equals(rgn.Name, region, StringComparison.OrdinalIgnoreCase) &&
								string.Equals(city.Subregion, subregion, StringComparison.OrdinalIgnoreCase)

							orderby city.Default descending, city.Name
							select city.Name;

				return Ok(query.Distinct().ToList());
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("city")]
		public IActionResult GetCity(string region, string subregion, string city)
		{
			try
			{
				City city2 = _GetCity(region, subregion, city);
				return Ok(city2);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("grid")]
		public IActionResult GetGridCoordinates([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city)
		{
			try
			{
				var grid = _GetGridCoordinates(region, subregion, city);
				return Ok(grid);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		internal Region GetRegion(string region)
		{
			Init();

			Region region2 = _regions.Where((Region rgn) => rgn.Name == region).FirstOrDefault();
			if (region2 == null)
				throw new Exception($"Could not find any region named '{region}'");

			return region2;
		}

		internal void ValidateSubregion(string region, string subregion)
		{
			GetRegion(region);
			int num = (from rgn in _regions
					   from c in rgn.Cities

					   where
						string.Equals(rgn.Name, region, StringComparison.OrdinalIgnoreCase) &&
						string.Equals(c.Subregion, subregion, StringComparison.OrdinalIgnoreCase)

					   select 1).Count();

			if (num < 1)
				throw new Exception($"Could not find any subregion named '{subregion}' in region '{region}'");
		}

		internal City _GetCity(string region, string subregion, string city)
		{
			ValidateSubregion(region, subregion);

			City city2 = (from rgn in _regions
						from c in rgn.Cities
							
						where
							string.Equals(rgn.Name, region, StringComparison.OrdinalIgnoreCase) &&
							string.Equals(c.Subregion, subregion, StringComparison.OrdinalIgnoreCase) &&
							string.Equals(c.Name, city, StringComparison.OrdinalIgnoreCase)

						  select c).FirstOrDefault();

			if (city2 == null)
				throw new Exception($"Could not find any city named '{city}' in region '{region}', subregion '{subregion}'");

			return city2;
		}

		internal GridCoordinates _GetGridCoordinates(string region, string subregion, string city)
		{
			Region region2 = GetRegion(region);
			City city2 = _GetCity(region, subregion, city);

			if (region2.MinLat >= city2.Latitude || city2.Latitude >= region2.MaxLat)
				throw new Exception($"City '{city}' has latitude outside region '{region}'");

			if (region2.MinLon >= city2.Longitude || city2.Longitude >= region2.MaxLon)
				throw new Exception($"City '{city}' has longitude outside region '{region}'");

			int num = 1 + (int)((region2.MaxLon - region2.MinLon) / region2.GridResolution);
			int num2 = 1 + (int)((region2.MaxLat - region2.MinLat) / region2.GridResolution);
			int c = (int)((double)num * (city2.Longitude - region2.MinLon) / (region2.MaxLon - region2.MinLon));
			int r = (int)((double)num2 * (region2.MaxLat - city2.Latitude) / (region2.MaxLat - region2.MinLat));

			return new GridCoordinates
			{
				C = c,
				R = r
			};
		}
	}
}