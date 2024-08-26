using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Exceptions;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Meteo;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]

    public class GeographyController : ApiControllerBase
    {
        private string _configFilePath;
        private List<Region> _regions;

        public GeographyController(IWebHostEnvironment hostingEnvironment, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            Init();
        }

        private void Init()
        {
            if (_regions?.Count > 0)
                return;

            _configFilePath = null;
            _regions = new List<Region>();
            _configFilePath = Path.Combine(_hostingEnvironment.ContentPath(), $"meteo/GeographicData.json");

            ReadConfigFile();

            var watchConfigFile = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_configFilePath),
                Filter = Path.GetFileName(_configFilePath),
                EnableRaisingEvents = true
            };

            watchConfigFile.Created += OnFileChanged;
            watchConfigFile.Changed += OnFileChanged;
        }

        private void OnFileChanged(object s, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Created ||
                    e.ChangeType == WatcherChangeTypes.Changed)
                {
                    ReadConfigFile();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void ReadConfigFile()
        {
            string value = System.IO.File.ReadAllText(_configFilePath);
            _regions = JsonSerializer.Deserialize<List<Region>>(value);
        }

        [HttpGet("regions")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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

        [HttpGet("cities/all")]
        [ProducesResponseType(typeof(List<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetAllCities()
        {
            try
            {
                var query = from rgn in _regions
                            from city in rgn.Cities
                            orderby city.Name
                            select new City
                            {
                                Default = city.Default,
                                Name = city.Name,
                                Latitude = city.Latitude,
                                Longitude = city.Longitude,
                                Region = rgn.Name,
                                Subregion = city.Subregion
                            };

                return Ok(query.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("city")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetCity(string region, string subregion, string city)
        {
            try
            {
                City city2 = InternalGetCity(region, subregion, city);
                return Ok(city2);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("grid")]
        [ProducesResponseType(typeof(GridCoordinates), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetGridCoordinates([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city)
        {
            try
            {
                var grid = InternalGetGridCoordinates(region, subregion, city);
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

            Region region2 = _regions.Find(rgn => rgn.Name == region);
            return region2 ?? throw new ExtendedException($"Could not find any region named '{region}'");
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
                throw new ExtendedException($"Could not find any subregion named '{subregion}' in region '{region}'");
        }

        internal City InternalGetCity(string region, string subregion, string city)
        {
            ValidateSubregion(region, subregion);

            City city2 = (from rgn in _regions
                          from c in rgn.Cities

                          where
                              string.Equals(rgn.Name, region, StringComparison.OrdinalIgnoreCase) &&
                              string.Equals(c.Subregion, subregion, StringComparison.OrdinalIgnoreCase) &&
                              string.Equals(c.Name, city, StringComparison.OrdinalIgnoreCase)

                          select c).FirstOrDefault();

            return city2 == null
                ? throw new ExtendedException($"Could not find any city named '{city}' in region '{region}', subregion '{subregion}'")
                : new City
                {
                    Default = city2.Default,
                    Name = city2.Name,
                    Latitude = city2.Latitude,
                    Longitude = city2.Longitude,
                    Region = region,
                    Subregion = city2.Subregion
                };
        }

        internal GridCoordinates InternalGetGridCoordinates(string region, string subregion, string city)
        {
            Region region2 = GetRegion(region);
            City city2 = InternalGetCity(region, subregion, city);

            if (region2.MinLat >= city2.Latitude || city2.Latitude >= region2.MaxLat)
                throw new ExtendedException($"City '{city}' has latitude outside region '{region}'");

            if (region2.MinLon >= city2.Longitude || city2.Longitude >= region2.MaxLon)
                throw new ExtendedException($"City '{city}' has longitude outside region '{region}'");

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