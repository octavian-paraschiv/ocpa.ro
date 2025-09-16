using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]

    public class GeographyController : ApiControllerBase
    {
        #region Private members
        private readonly IGeographyService _geographyService;
        #endregion

        #region Constructor (DI)
        public GeographyController(IGeographyService geographyHelper, ILogger logger)
            : base(logger)
        {
            _geographyService = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
        }
        #endregion

        #region Public controller methods

        [HttpGet("regions/names")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetRegionNames")]
        public IActionResult GetRegionNames()
        {
            try
            {
                return Ok(_geographyService.GetRegionNames());
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("subregions/names")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetSubregionNames")]
        public IActionResult GetSubregionNames([FromQuery] string region)
        {
            try
            {
                return Ok(_geographyService.GetSubregionNames(region));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cities/names")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetCityNames")]
        public IActionResult GetCityNames([FromQuery] string region, [FromQuery] string subregion)
        {
            try
            {
                return Ok(_geographyService.GetCityNames(region, subregion));
            }
            catch (Exception ex)
            {

                LogException(ex);
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("regions/all")]
        [ProducesResponseType(typeof(IEnumerable<RegionDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetAllRegions")]
        public IActionResult GetAllRegions()
        {
            try
            {
                return Ok(_geographyService.GetAllRegions());
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cities/all")]
        [ProducesResponseType(typeof(List<CityDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetAllCities")]
        public IActionResult GetAllCities()
        {
            try
            {
                return Ok(_geographyService.GetAllCities());
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("city")]
        [ProducesResponseType(typeof(CityDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetCity")]
        public IActionResult GetCity([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city)
        {
            try
            {
                return Ok(_geographyService.GetCity(region, subregion, city));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("grid")]
        [ProducesResponseType(typeof(GridCoordinates), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetGridCoordinates")]
        public IActionResult GetGridCoordinates([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city)
        {
            try
            {
                return Ok(_geographyService.GetGridCoordinates(region, subregion, city));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("geolocation/{ipAddress}")]
        [ProducesResponseType(typeof(GeoLocation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetGeoLocation")]
        [Authorize(Roles = "ADM")]
        [IgnoreWhenNotInDev]
        public async Task<IActionResult> GetGeoLocation([FromRoute] string ipAddress)
        {
            try
            {
                return Ok(await _geographyService.GetGeoLocation(ipAddress));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("city/save")]
        [ProducesResponseType(typeof(CityDetail), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CityDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "SaveCity")]
        public IActionResult SaveApplication([FromBody] CityDetail city)
        {
            try
            {
                var dbu = _geographyService.SaveCity(city, out bool inserted);
                if (dbu != null)
                    return inserted ?
                        StatusCode(StatusCodes.Status201Created, dbu) :
                        Ok(dbu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest();
        }

        [HttpPost("city/delete/{cityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "DeleteCity")]
        public IActionResult DeleteApplication([FromRoute] int cityId)
        {
            try
            {
                return StatusCode(_geographyService.DeleteCity(cityId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}