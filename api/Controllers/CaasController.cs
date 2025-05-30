using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Content;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "RefreshCat")]
        public async Task<IActionResult> RefreshCat([FromRoute] string resourcePath)
        {
            try
            {
                await _helper.RefreshCat(resourcePath, Request.QueryString.Value);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return BadRequest($"Cannot refresh cat from resource: {resourcePath}");
        }
    }
}
