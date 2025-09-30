using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Extensions;
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
    [ApiExplorerSettings(GroupName = "Utility")]
    public class UtilityController : ApiControllerBase
    {
        public UtilityController(ILogger logger)
            : base(logger)
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

        [HttpPost("encode")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "Encode")]
        [IgnoreWhenNotInDev]
        public IActionResult Encode([FromBody] string[] data)
            => Ok(StringUtility.EncodeStrings(data ?? []));

        [HttpPost("decode")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "Decode")]
        [IgnoreWhenNotInDev]
        public IActionResult Decode([FromBody] string data)
           => Ok(StringUtility.DecodeStrings(data));
    }
}
