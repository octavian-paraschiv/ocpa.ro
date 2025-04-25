using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Geography;
using ocpa.ro.api.Models.Geography;
using ocpa.ro.api.Policies;
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
        private readonly IGeographyHelper _geographyHelper;
        #endregion

        #region Constructor (DI)
        public GeographyController(IWebHostEnvironment hostingEnvironment, IGeographyHelper geographyHelper, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            _geographyHelper = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
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
                return Ok(_geographyHelper.GetRegionNames());
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
                return Ok(_geographyHelper.GetSubregionNames(region));
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
                return Ok(_geographyHelper.GetCityNames(region, subregion));
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
                return Ok(_geographyHelper.GetAllCities());
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
                return Ok(_geographyHelper.GetCity(region, subregion, city));
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
                return Ok(_geographyHelper.GetGridCoordinates(region, subregion, city));
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
                return Ok(await _geographyHelper.GetGeoLocation(ipAddress));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("city/save")]
        [ProducesResponseType(typeof(City), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "SaveCity")]
        public IActionResult SaveApplication([FromBody] City city)
        {
            try
            {
                var dbu = _geographyHelper.SaveCity(city, out bool inserted);
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
        [SwaggerOperation(OperationId = "DeleteCity")]
        public IActionResult DeleteApplication([FromRoute] int cityId)
        {
            try
            {
                return StatusCode(_geographyHelper.DeleteCity(cityId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}