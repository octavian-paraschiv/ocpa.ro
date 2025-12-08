using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Extensions;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private readonly IHostingEnvironmentService _hostingEnvironment;

        public UtilityController(ILogger logger, IHostingEnvironmentService hostingEnvironment)
            : base(logger)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
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

        [HttpGet("config")]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "GetConfig")]
        public IActionResult GetConfig()
        {
            var variables = Environment.GetEnvironmentVariables() ?? new Dictionary<string, string>();
            var x = variables.Keys
                .Cast<string>()
                .Select(key => new KeyValuePair<string, string>(key, variables[key]?.ToString() ?? "<null>"))
                .DistinctBy(x => x.Key)
                .ToDictionary();

            x.Add("ContentPath", _hostingEnvironment.ContentPath);
            x.Add("ContentRootPath", _hostingEnvironment.ContentRootPath);

            return Ok(x);
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
