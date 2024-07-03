using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Content;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Models.Content;
using ocpa.ro.api.Policies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "Admin")]
    public class ContentController : ApiControllerBase
    {
        private readonly IContentHelper _contentHelper;
        private readonly IMultipartRequestHelper _multipartHelper = null;

        public ContentController(IWebHostEnvironment hostingEnvironment, IContentHelper contentHelper, IMultipartRequestHelper multipartHelper) : base(hostingEnvironment)
        {
            _contentHelper = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
        }

        [HttpGet("paths")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        public IActionResult GetContentRootPath()
        {
            return Ok(new Dictionary<string, string>
            {
                { "_hostingEnvironment.ContentRootPath", _hostingEnvironment.ContentRootPath },
                { "_hostingEnvironment.ContentPath()", _hostingEnvironment.ContentPath() }
            });
        }

        [HttpGet("{*contentPath}")]
        [ProducesResponseType(typeof(ContentUnit), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetContent([FromRoute] string contentPath, [FromQuery] int? level = null, [FromQuery] string filter = null)
        {
            try
            {
                var content = _contentHelper.ListContent(contentPath, level, filter);
                if ((content?.Type ?? ContentUnitType.None) != ContentUnitType.None)
                    return Ok(content);
            }
            catch
            {
            }

            return NotFound(contentPath);
        }

        [HttpPost("{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadContent([FromRoute] string contentPath)
        {
            try
            {
                byte[] data = await _multipartHelper.GetMultipartRequestData(Request);

                var ucu = await _contentHelper.CreateOrUpdateContent(contentPath, data);
                if (ucu == null)
                    return NotFound(contentPath);

                return StatusCode((int)ucu.StatusCode, ucu.StatusCode.IsSuccess() ? (object)ucu : contentPath);
            }
            catch
            {
                return Conflict(contentPath);
            }
        }

        [HttpDelete("{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public IActionResult DeleteContent([FromRoute] string contentPath)
        {
            try
            {
                var status = _contentHelper.DeleteContent(contentPath);
                return StatusCode((int)status, contentPath);
            }
            catch
            {
                return Conflict(contentPath);
            }
        }
    }
}
