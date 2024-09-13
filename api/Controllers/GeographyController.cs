using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Geography;
using ocpa.ro.api.Models.Meteo;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;

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
        [ProducesResponseType(typeof(List<City>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
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

        #endregion
    }
}