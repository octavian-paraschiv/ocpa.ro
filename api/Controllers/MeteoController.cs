using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Meteo;
using ocpa.ro.api.Models.Meteo;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MeteoController : ApiControllerBase
    {
        const long MaxFileSize = 16 * 1024L * 1024L;

        private readonly IMeteoDataHelper _dataHelper = null;

        public MeteoController(IWebHostEnvironment hostingEnvironment, IMeteoDataHelper meteoDataHelper)
            : base(hostingEnvironment)
        {
            _dataHelper = meteoDataHelper;
        }


        [Authorize]
        [HttpPost("uploadPart")]
        [RequestSizeLimit(MaxFileSize)]
        [ProducesResponseType(typeof(UploadDataPartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public IActionResult UploadPart([FromBody] UploadDataPart part)
        {
            try
            {
                var rsp = _dataHelper.HandleDatabasePart(part);
                return Ok(rsp);
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized(uae.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(CalendarRange), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetRange()
        {
            try
            {
                var range = _dataHelper.GetCalendarRange(0);
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
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                GridCoordinates gridCoordinates = new GeographyController(_hostingEnvironment).InternalGetGridCoordinates(region, subregion, city);
                var data = _dataHelper.GetMeteoData(gridCoordinates, region, skip, take);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
