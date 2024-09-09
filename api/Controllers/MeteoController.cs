using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Generic;
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
        private readonly IMeteoDataHelper _dataHelper = null;
        private readonly IMultipartRequestHelper _multipartHelper = null;

        public MeteoController(IWebHostEnvironment hostingEnvironment, IMeteoDataHelper meteoDataHelper, IMultipartRequestHelper multipartHelper, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            _dataHelper = meteoDataHelper ?? throw new ArgumentNullException(nameof(meteoDataHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
        }

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

        [HttpGet("data")]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetMeteoData", Description = "Get Meteo Data")]
        public IActionResult GetMeteoData([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
           [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = new GeographyController(_hostingEnvironment, _logger).InternalGetGridCoordinates(region, subregion, city);
                var data = _dataHelper.GetMeteoData(true, gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("data/preview/{dbi}")]
        [Authorize(Roles = "ADM")]
        [IgnoreWhenNotInDev]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetPreviewMeteoData", Description = "Get Preview Meteo Data (by index)")]
        public IActionResult GetMeteoData([FromRoute] int dbi, [FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                _dataHelper.PromotePreviewDatabase(new PromoteDatabaseModel
                {
                    Dbi = dbi,
                    Operational = false
                });

                GridCoordinates gridCoordinates = new GeographyController(_hostingEnvironment, _logger).InternalGetGridCoordinates(region, subregion, city);
                var data = _dataHelper.GetMeteoData(false, gridCoordinates, region, skip, take);
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
        [SwaggerOperation(OperationId = "UploadPreviewDatabase", Description = "Upload Preview Database (index 0)")]
        public Task<IActionResult> UploadPreviewDatabase() => UploadPreviewDatabaseByIndex(0);


        [Authorize(Roles = "ADM")]
        [HttpPost("database/preview/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(OperationId = "UploadPreviewDatabaseByIndex", Description = "Upload a Preview Database (by index)")]
        public async Task<IActionResult> UploadPreviewDatabaseByIndex([FromRoute] int dbi)
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
        [HttpPost("database/preview/promote")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [IgnoreWhenNotInDev]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [SwaggerOperation(OperationId = "PromotePreviewDatabase", Description = "Promote a Preview Database")]
        public IActionResult PromotePreviewDatabase([FromBody] PromoteDatabaseModel promote)
        {
            try
            {
                _dataHelper.PromotePreviewDatabase(promote);
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
