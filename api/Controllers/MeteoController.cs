﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Helpers.Meteo;
using ocpa.ro.api.Models.Meteo;
using ocpa.ro.api.Policies;
using Serilog;
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
        [ApiExplorerIgnore]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
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

        [HttpGet("range")]
        [ProducesResponseType(typeof(CalendarRange), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetRange() => GetRange(0);

        [HttpGet("range/{dbi}")]
        [Authorize(Roles = "ADM")]
        [ApiExplorerIgnore]
        [ProducesResponseType(typeof(CalendarRange), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetRange([FromRoute] int dbi)
        {
            try
            {
                var range = _dataHelper.GetCalendarRange(dbi, 0);
                return Ok(range);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("data")]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetMeteoData([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city,
           [FromQuery] int skip = 0, [FromQuery] int take = 10) => GetMeteoData(region, subregion, city, 0, skip, take);


        [HttpGet("data/{dbi}")]
        [Authorize(Roles = "ADM")]
        [ApiExplorerIgnore]
        [ProducesResponseType(typeof(MeteoData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetMeteoData([FromQuery] string region, [FromQuery] string subregion, [FromQuery] string city, [FromRoute] int dbi,
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = new GeographyController(_hostingEnvironment, _logger).InternalGetGridCoordinates(region, subregion, city);
                var data = _dataHelper.GetMeteoData(dbi, gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Roles = "API")]
        [HttpPost("database")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ApiExplorerIgnore]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        public Task<IActionResult> UploadDatabase() => UploadDatabase(0);

        [Authorize(Roles = "ADM")]
        [HttpPost("database/{dbi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ApiExplorerIgnore]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDatabase([FromRoute] int dbi)
        {
            try
            {
                byte[] data = await _multipartHelper.GetMultipartRequestData(Request);
                await _dataHelper.ReplaceDatabase(dbi, data);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
