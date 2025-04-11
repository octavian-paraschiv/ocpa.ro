using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Content;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Helpers.Wiki;
using ocpa.ro.api.Models.Content;
using ocpa.ro.api.Policies;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "ADM")]
    public class ContentController : ApiControllerBase
    {
        #region Private members
        private readonly IContentHelper _contentHelper;
        private readonly IMultipartRequestHelper _multipartHelper;
        private readonly IWikiHelper _wikiHelper;
        #endregion

        #region Constructor (DI)
        public ContentController(IWebHostEnvironment hostingEnvironment, ILogger logger,
            IContentHelper contentHelper, IWikiHelper wikiHelper, IMultipartRequestHelper multipartHelper)
            : base(hostingEnvironment, logger, null)
        {
            _contentHelper = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
            _wikiHelper = wikiHelper ?? throw new ArgumentNullException(nameof(wikiHelper));
        }
        #endregion

        #region Public controller methods
        [HttpGet("{*contentPath}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/plain")]
        [SwaggerOperation(OperationId = "GetContent")]
        public async Task<IActionResult> GetContent([FromRoute] string contentPath)
        {
            IActionResult result = NotFound(contentPath);

            try
            {
                var content = await _contentHelper.GetContent(contentPath);
                if (content?.Length > 0)
                    result = Ok(Convert.ToBase64String(content));
            }
            catch (Exception ex)
            {
                LogException(ex);
                result = NotFound(contentPath);
            }

            return result;
        }

        [HttpGet("list/{*contentPath}")]
        [ProducesResponseType(typeof(ContentUnit), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = "ListContent")]
        public IActionResult ListContent([FromRoute] string contentPath, [FromQuery] int? level = null,
            [FromQuery] string filter = null)
        {
            IActionResult result = NotFound(contentPath);

            try
            {
                var content = _contentHelper.ListContent(contentPath, level, filter);
                if ((content?.Type ?? ContentUnitType.None) != ContentUnitType.None)
                    result = Ok(content);
            }
            catch (Exception ex)
            {
                LogException(ex);
                result = NotFound(contentPath);
            }

            return result;
        }

        [HttpPost("plain/{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(Constants.MaxPlainRequestSize)]
        [Consumes("text/plain")]
        [SwaggerOperation(OperationId = "UploadPlainContent")]
        public async Task<IActionResult> UploadPlainContent([FromRoute] string contentPath)
        {
            try
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var content = Encoding.UTF8.GetBytes(await reader.ReadToEndAsync());
                var ucu = await _contentHelper.CreateOrUpdateContent(contentPath, content);
                if (ucu == null)
                    return NotFound(contentPath);

                return StatusCode((int)ucu.StatusCode, ucu.StatusCode.IsSuccess() ? ucu : contentPath);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Conflict(contentPath);
            }
        }

        [HttpPost("multipart/{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(Constants.MaxMultipartRequestSize)]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(OperationId = "UploadMultipartContent")]
        public async Task<IActionResult> UploadMultipartContent([FromRoute] string contentPath)
        {
            try
            {
                byte[] data = await _multipartHelper.GetMultipartRequestData(Request);

                var ucu = await _contentHelper.CreateOrUpdateContent(contentPath, data);
                if (ucu == null)
                    return NotFound(contentPath);

                return StatusCode((int)ucu.StatusCode, ucu.StatusCode.IsSuccess() ? ucu : contentPath);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Conflict(contentPath);
            }
        }

        [HttpPost("delete/{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [SwaggerOperation(OperationId = "DeleteContent")]
        public IActionResult DeleteContent([FromRoute] string contentPath)
        {
            try
            {
                var status = _contentHelper.DeleteContent(contentPath);
                return StatusCode((int)status, contentPath);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Conflict(contentPath);
            }
        }

        [HttpGet("render/{*resourcePath}")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = "RenderContent")]
        public async Task<IActionResult> RenderContent([FromRoute] string resourcePath,
           [FromHeader(Name = "X-RenderAsHtml")] string renderAsHtmlStr)
        {
            try
            {
                string reqUrl = Request.GetDisplayUrl();
                string reqPath = Request.Path;
                string reqRoot = reqUrl.Replace(reqPath, string.Empty);

                bool renderAsHtml = !string.Equals(renderAsHtmlStr, "FALSE", StringComparison.OrdinalIgnoreCase);

                var ext = Path.GetExtension(resourcePath);
                if (ext?.Length > 0)
                {
                    var data = await _wikiHelper.ProcessWikiResource(resourcePath, reqRoot,
                        RequestLanguage, renderAsHtml).ConfigureAwait(false);

                    if (data?.Length > 0)
                        return File(data, "application/octet-stream");
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return NotFound($"Resource not found: {resourcePath}");
        }
        #endregion
    }
}
