using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Content;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Models.Content;
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
    [Authorize(Roles = "ADM")]
    public class ContentController : ApiControllerBase
    {
        #region Private members
        private readonly IContentHelper _contentHelper;
        private readonly IMultipartRequestHelper _multipartHelper;
        #endregion

        #region Constructor (DI)
        public ContentController(IWebHostEnvironment hostingEnvironment, ILogger logger,
            IContentHelper contentHelper, IMultipartRequestHelper multipartHelper)
            : base(hostingEnvironment, logger, null)
        {
            _contentHelper = contentHelper ?? throw new ArgumentNullException(nameof(contentHelper));
            _multipartHelper = multipartHelper ?? throw new ArgumentNullException(nameof(multipartHelper));
        }
        #endregion

        #region Public controller methods
        [HttpGet("{*contentPath}")]
        [ProducesResponseType(typeof(ContentUnit), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = "GetContent")]
        public IActionResult GetContent([FromRoute] string contentPath, [FromQuery] int? level = null,
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

        [HttpPost("{*contentPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MultipartRequestHelper.MaxFileSize)]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(OperationId = "UploadContent")]
        public async Task<IActionResult> UploadContent([FromRoute] string contentPath)
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
        #endregion
    }
}
