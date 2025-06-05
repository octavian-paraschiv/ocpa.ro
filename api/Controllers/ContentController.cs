using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Helpers.Content;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Models.Configuration;
using ocpa.ro.api.Models.Content;
using ocpa.ro.api.Policies;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
using System.Linq;
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
        private readonly IContentRenderer _contentRenderer;
        private readonly IDistributedCache _cache;
        private readonly CacheConfig _config;
        #endregion

        #region Constructor (DI)
        public ContentController(IWebHostEnvironment hostingEnvironment, ILogger logger, IAuthHelper authHelper,
            IContentHelper contentHelper, IContentRenderer wikiHelper, IMultipartRequestHelper multipartHelper,
            IDistributedCache cache, IOptions<CacheConfig> config)
            : base(hostingEnvironment, logger, authHelper)
        {
            _contentHelper = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
            _contentRenderer = wikiHelper ?? throw new ArgumentNullException(nameof(wikiHelper));

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
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
        [AllowAnonymous]
        public IActionResult ListContent([FromRoute] string contentPath,
            [FromQuery] int? level = null,
            [FromQuery] string filter = null,
            [FromQuery] bool? markdownView = null)
        {
            _authHelper.GuardContentPath(HttpContext.User?.Identity, contentPath);

            IActionResult result = NotFound(contentPath);

            try
            {
                var content = _contentHelper.ListContent(contentPath, level, filter, markdownView ?? false);
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

        [HttpPost("folder/{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [SwaggerOperation(OperationId = "CreateNewFolder")]
        public IActionResult CreateNewFolder([FromRoute] string contentPath)
        {
            try
            {
                var ucu = _contentHelper.CreateNewFolder(contentPath);
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

        [HttpPost("upload/{*contentPath}")]
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

        [HttpPost("move/{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [SwaggerOperation(OperationId = "MoveContent")]
        public IActionResult RenameContent([FromRoute] string contentPath, [FromQuery] string newPath)
        {
            try
            {
                var ucu = _contentHelper.MoveContent(contentPath, newPath);
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

        [HttpGet("render/{*resourcePath}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = "RenderContent")]
        [AllowAnonymous]
        public async Task<IActionResult> RenderContent([FromRoute] string resourcePath,
           [FromHeader(Name = "X-RenderAsHtml")] string renderAsHtmlStr,
           [FromHeader(Name = "X-UseCache")] string useCacheStr)
        {
            _authHelper.GuardContentPath(HttpContext.User?.Identity, resourcePath);

            try
            {
                byte[] data = [];
                bool isBinary = false;

                bool renderAsHtml = !string.Equals(renderAsHtmlStr, "FALSE", StringComparison.OrdinalIgnoreCase);
                bool useCache = !string.Equals(useCacheStr, "FALSE", StringComparison.OrdinalIgnoreCase);
                string cacheKey = $"{RequestLanguage ?? string.Empty}:{resourcePath}".TrimStart(':');

                if (useCache)
                {
                    var rawData = await _cache.GetAsync(cacheKey);
                    if (rawData?.Length > 1)
                    {
                        isBinary = rawData[0] != 0;
                        data = rawData.Skip(1).ToArray();
                    }
                }

                if (!(data?.Length > 0))
                {
                    string reqUrl = Request.GetDisplayUrl();
                    string reqPath = Request.Path;
                    string reqRoot = reqUrl.Replace(reqPath, string.Empty);

                    var ext = Path.GetExtension(resourcePath);
                    if (ext?.Length > 0)
                    {
                        (data, isBinary) = await _contentRenderer.RenderContent(resourcePath, reqRoot,
                            RequestLanguage, renderAsHtml).ConfigureAwait(false);

                        if (useCache)
                        {
                            if (data?.Length > (_config.MinSizeKB * 1024)) // only cache "large" content
                            {
                                byte[] start = [Convert.ToByte(isBinary)];
                                byte[] cacheData = [.. start, .. data];

                                await _cache.SetAsync(cacheKey, cacheData, new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CachePeriod)
                                });
                            }
                            else
                            {
                                await _cache.RemoveAsync(cacheKey);
                            }
                        }
                    }
                }

                if (data?.Length > 0)
                {
                    IActionResult res = (renderAsHtml && !isBinary) ?
                        Content(Encoding.UTF8.GetString(data), "text/html") :
                        File(data, "application/octet-stream");

                    return res;
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
