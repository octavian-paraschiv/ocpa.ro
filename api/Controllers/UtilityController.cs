using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Email;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    public class SendMailRequest
    {
        public string[] Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]

    public class UtilityController : ApiControllerBase
    {
        private IEmailHelper _emailHelper;

        public UtilityController(IWebHostEnvironment hostingEnvironment, ILogger logger, IEmailHelper emailHelper)
            : base(hostingEnvironment, logger, null)
        {
            _emailHelper = emailHelper ?? throw new ArgumentNullException(nameof(emailHelper));
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

        [HttpPost("send-mail")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "SendMail")]
        public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
        {
            try
            {
                var err = await _emailHelper.SendEmail(request.Recipients, request.Subject, request.Body);
                if (string.IsNullOrEmpty(err))
                    return Ok();

                return BadRequest(err);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
