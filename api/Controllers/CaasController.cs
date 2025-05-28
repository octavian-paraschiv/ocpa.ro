using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Wiki;
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
    public class CaasController : ApiControllerBase
    {
        private readonly ICaasHelper _helper;

        public CaasController(IWebHostEnvironment hostingEnvironment, ILogger logger,
            ICaasHelper helper)
            : base(hostingEnvironment, logger, null)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        [HttpGet("{*resourcePath}")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "image/gif")]
        [SwaggerOperation(OperationId = "GetNewCat")]
        public async Task<IActionResult> GetNewCat([FromRoute] string resourcePath, [FromQuery] bool? refreshCat)
        {
            try
            {
                var data = await _helper.GetNewCat(resourcePath, Request.QueryString.Value, refreshCat ?? false);
                if (data?.Length > 0)
                    return File(data, "image/gif");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return NotFound($"Resource not found: {resourcePath}");
        }
    }
}
