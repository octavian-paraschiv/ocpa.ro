using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Entities.Meteo;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiExplorerSettings(GroupName = "Experimental")]
    public class Meteo2Controller : ApiControllerBase
    {
        #region Private members
        private readonly IContentService _contentService = null;
        private readonly IMeteoDataService2 _meteoDataService2 = null;
        private readonly IGeographyService _geographyService = null;
        private readonly IMultipartRequestService _multipartRequestService = null;
        #endregion

        #region Constructor (DI)
        public Meteo2Controller(IMeteoDataService2 meteoDataService2,
            IGeographyService geographyHelper,
            IMultipartRequestService multipartRequestService,
            IContentService contentService,
            ILogger logger)
            : base(logger)
        {
            _meteoDataService2 = meteoDataService2 ?? throw new ArgumentNullException(nameof(meteoDataService2));
            _geographyService = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
            _multipartRequestService = multipartRequestService ?? throw new ArgumentNullException(nameof(multipartRequestService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        }
        #endregion



        [Authorize(Roles = "API,ADM")]
        [HttpGet("databases/all")]
        [ProducesResponseType(typeof(MeteoDbInfo[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllDatabases", Description = "Get a list with all databases")]
        public IActionResult GetAllDatabases()
        {
            try
            {
                var info = _meteoDataService2.GetDatabases();
                return Ok(info);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("data")]
        [AllowAnonymous]
        [IgnoreWhenNotInDev]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(OperationId = "GetActiveMeteoData", Description = "Get Active Meteo Data")]
        public IActionResult GetActiveMeteoData(
            [FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = _geographyService.GetGridCoordinates(region, subregion, city);
                var data = _meteoDataService2.GetMeteoData(-1, gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("data/{dbi}")]
        [Authorize(Roles = "ADM")]
        [IgnoreWhenNotInDev]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(OperationId = "GetMeteoData", Description = "Get Meteo Data")]
        public IActionResult GetMeteoData(
            [FromRoute] int dbi,
            [FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = _geographyService.GetGridCoordinates(region, subregion, city);
                var data = _meteoDataService2.GetMeteoData(dbi, gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADM")]
        [HttpPost("database/activate/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "ActivateDatabaseSnapshot", Description = "Activate Database Snapshot")]
        public IActionResult ActivateDatabaseSnapshot([FromRoute] int dbi)
        {
            try
            {
                _meteoDataService2.MakeActiveDbi(dbi);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "API,ADM")]
        [HttpPost("data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "SaveMeteoData", Description = "Save Meteo Data")]
        public IActionResult SaveMeteoData(
            [FromBody] IEnumerable<MeteoDbData> data,
            [FromQuery] bool? purgeDbiRecords)
        {
            try
            {
                _meteoDataService2.SaveMeteoData(data, purgeDbiRecords ?? false);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
