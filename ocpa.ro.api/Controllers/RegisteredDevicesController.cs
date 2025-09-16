using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Entities;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("registered-devices")]
    [ApiController]
    [Authorize(Roles = "ADM")]
    [IgnoreWhenNotInDev]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RegisteredDevicesController : ApiControllerBase
    {
        private readonly IAccessService _accessService;

        public RegisteredDevicesController(IAccessService accessService, ILogger logger)
            : base(logger)
        {
            _accessService = accessService ?? throw new ArgumentNullException(nameof(accessService));
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(RegisteredDevice[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllRegisteredDevices")]
        public IActionResult GetAllRegisteredDevices()
        {
            try
            {
                return Ok(_accessService.GetRegisteredDevices());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("delete/{deviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeleteRegisteredDevice")]
        public IActionResult DeleteRegisteredDevice([FromRoute] string deviceId)
        {
            try
            {
                return StatusCode(_accessService.DeleteRegisteredDevice(deviceId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
