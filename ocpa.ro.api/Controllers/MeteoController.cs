using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Constants;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiExplorerSettings(GroupName = "Meteo")]
    public class MeteoController : ApiControllerBase
    {
        #region Private members
        private readonly IContentService _contentService = null;
        private readonly IMeteoDataService _meteoDataService = null;
        private readonly IGeographyService _geographyService = null;
        private readonly IMultipartRequestService _multipartRequestService = null;
        #endregion

        #region Constructor (DI)
        public MeteoController(IMeteoDataService meteoDataService,
            IGeographyService geographyHelper,
            IMultipartRequestService multipartRequestService,
            IContentService contentService,
            ILogger logger)
            : base(logger)
        {
            _meteoDataService = meteoDataService ?? throw new ArgumentNullException(nameof(meteoDataService));
            _geographyService = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
            _multipartRequestService = multipartRequestService ?? throw new ArgumentNullException(nameof(multipartRequestService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        }
        #endregion

        #region Public controller methods
        [HttpGet("studio-download-url")]
        [Authorize(Roles = "ADM")]
        [IgnoreWhenNotInDev]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = "GetStudioDownloadUrl", Description = "Get Studio Download Url")]
        public IActionResult GetStudioDownloadUrl()
        {
            try
            {
                var fileName = _contentService.LatestThorusStudioFile;
                if (fileName?.Length > 0)
                    return Ok($"{Request.Scheme}://{Request.Host}/content/Meteo/current/{fileName}");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return NotFound();
        }

        [HttpGet("data")]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetMeteoData", Description = "Get Meteo Data")]
        public Task<IActionResult> GetMeteoData(
            [FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)

            => GetMeteoData(-1, region, subregion, city, skip, take);


        [HttpGet("data/preview/{dbi}")]
        [Authorize(Roles = "ADM")]
        [IgnoreWhenNotInDev]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetPreviewMeteoData", Description = "Get Preview Meteo Data (by index)")]
        public async Task<IActionResult> GetMeteoData(
            [FromRoute] int dbi,
            [FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = _geographyService.GetGridCoordinates(region, subregion, city);
                var data = await _meteoDataService.GetMeteoData(dbi, gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "API,ADM")]
        [HttpPost("database/upload/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(AppConstants.MaxMultipartRequestSize)]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(OperationId = "UploadPreviewDatabaseByIndex", Description = "Upload a Preview Database (by index)")]
        public async Task<IActionResult> UploadPreviewDatabaseByDbi([FromRoute] int dbi)
        {
            try
            {
                byte[] data = await _multipartRequestService.GetMultipartRequestData(Request);
                await _meteoDataService.SavePreviewDatabase(dbi, data);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADM")]
        [HttpPost("database/delete/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeletePreviewDatabaseByIndex", Description = "Delete a Preview Database (by index)")]
        public async Task<IActionResult> DeletePreviewDatabaseByIndex([FromRoute] int dbi)
        {
            try
            {
                await _meteoDataService.DropPreviewDatabase(dbi);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADM")]
        [HttpPost("database/promote/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "PromotePreviewDatabase", Description = "Promote a Preview Database")]
        public async Task<IActionResult> PromotePreviewDatabase([FromRoute] int dbi)
        {
            try
            {
                await _meteoDataService.PromotePreviewDatabase(dbi);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "API,ADM")]
        [HttpGet("databases/all")]
        [ProducesResponseType(typeof(MeteoDbInfo[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllDatabases", Description = "Get a list with all databases")]
        public async Task<IActionResult> GetAllDatabases()
        {
            try
            {
                var info = await _meteoDataService.GetDatabases();
                return Ok(info);
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
