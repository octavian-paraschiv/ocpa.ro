using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]

    public class UtilityController : ApiControllerBase
    {
        public UtilityController(IWebHostEnvironment hostingEnvironment, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
        }

        [HttpGet("keep-alive")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "KeepAlive")]
        public IActionResult KeepAlive()
        {
            return Ok("ok");
        }

        [HttpGet("backend-version")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "BackendVersion")]
        public IActionResult BackendVersion()
        {
            var location = typeof(Program).Assembly.Location;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);
            return Ok(fvi.FileVersion);
        }
    }
}
