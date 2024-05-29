using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Helpers.Meteo;
using ocpa.ro.api.Models.Meteo;
using ocpa.ro.api.Policies;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MeteoController : ApiControllerBase
    {
        private readonly IMeteoDataHelper _dataHelper = null;
        private readonly IMultipartRequestHelper _multipartHelper = null;

        public MeteoController(IWebHostEnvironment hostingEnvironment, IMeteoDataHelper meteoDataHelper, IMultipartRequestHelper multipartHelper)
            : base(hostingEnvironment)
        {
            _dataHelper = meteoDataHelper ?? throw new ArgumentNullException(nameof(meteoDataHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
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


        [Authorize(Roles = "ApiUser")]
        [HttpPost("uploadDatabase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        public async Task<IActionResult> UploadDatabase()
        {
            try
            {
                byte[] data = await _multipartHelper.GetMultipartRequestData(Request);
                await _dataHelper.ReplaceDatabase(data);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
