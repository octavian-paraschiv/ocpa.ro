using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Helpers.Geography;
using ocpa.ro.api.Helpers.Meteo;
using ocpa.ro.api.Models.Meteo;
using ocpa.ro.api.Policies;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MeteoController : ApiControllerBase
    {
        #region Private members
        private readonly IMeteoDataHelper _dataHelper = null;
        private readonly IGeographyHelper _geographyHelper = null;
        private readonly IMultipartRequestHelper _multipartHelper = null;
        #endregion

        #region Constructor (DI)
        public MeteoController(IWebHostEnvironment hostingEnvironment,
            IMeteoDataHelper meteoDataHelper, IGeographyHelper geographyHelper,
            IMultipartRequestHelper multipartHelper, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            _dataHelper = meteoDataHelper ?? throw new ArgumentNullException(nameof(meteoDataHelper));
            _geographyHelper = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
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
                var fileName = _dataHelper.LatestStudioFile;
                if (fileName?.Length > 0)
                    return Ok($"{Request.Scheme}://{Request.Host}/content/Meteo/current/{fileName}");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return NotFound();
        }

        [HttpGet("data/{region}/{subregion}/{city}")]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetMeteoData", Description = "Get Meteo Data")]
        public Task<IActionResult> GetMeteoData(
            [FromRoute] string region, [FromRoute] string subregion, [FromRoute] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)

            => GetMeteoData(-1, region, subregion, city, skip, take);


        [HttpGet("data/preview/{dbi}/{region}/{subregion}/{city}")]
        [Authorize(Roles = "ADM")]
        [IgnoreWhenNotInDev]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetPreviewMeteoData", Description = "Get Preview Meteo Data (by index)")]
        public async Task<IActionResult> GetMeteoData(
            [FromRoute] int dbi,
            [FromRoute] string region, [FromRoute] string subregion, [FromRoute] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = _geographyHelper.GetGridCoordinates(region, subregion, city);
                var data = await _dataHelper.GetMeteoData(dbi, gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "API")]
        [HttpPost("database/preview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(OperationId = "UploadPreviewDatabase", Description = "Upload Preview Database - for Thorus")]
        public Task<IActionResult> UploadPreviewDatabase()
            // By convention, databases uploaded via Thorus are always uploaded as Preview3.db3,
            // ie. in the last position
            => UploadPreviewDatabaseByDbi(MeteoDataHelper.DbCount - 2);


        [Authorize(Roles = "ADM")]
        [HttpPost("database/preview/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(OperationId = "UploadPreviewDatabaseByIndex", Description = "Upload a Preview Database (by index)")]
        public async Task<IActionResult> UploadPreviewDatabaseByDbi([FromRoute] int dbi)
        {
            try
            {
                byte[] data = await _multipartHelper.GetMultipartRequestData(Request);
                await _dataHelper.SavePreviewDatabase(dbi, data);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADM")]
        [HttpPost("database/preview/promote/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "PromotePreviewDatabase", Description = "Promote a Preview Database")]
        public async Task<IActionResult> PromotePreviewDatabase([FromRoute] int dbi)
        {
            try
            {
                await _dataHelper.PromotePreviewDatabase(dbi);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADM")]
        [HttpGet("databases/all")]
        [ProducesResponseType(typeof(MeteoDbInfo[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllDatabases", Description = "Get a list with all databases")]
        public async Task<IActionResult> GetAllDatabases()
        {
            try
            {
                var info = await _dataHelper.GetDatabases();
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
