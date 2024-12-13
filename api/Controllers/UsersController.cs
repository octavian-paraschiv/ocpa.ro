using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Models.Authentication;
using ocpa.ro.api.Models.Menus;
using ocpa.ro.api.Policies;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "ADM")]
    public class UsersController : ApiControllerBase
    {
        private readonly IJwtTokenHelper _jwtTokenGenerator;

        public UsersController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, IJwtTokenHelper jwtTokenGenerator, ILogger logger)
            : base(hostingEnvironment, logger, authHelper)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("authenticate")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "Authenticate")]
        public IActionResult Authenticate([FromForm] AuthenticateRequest model,
            [FromHeader(Name = "X-Device-Id")] string deviceId)
        {
            var user = _authHelper.AuthorizeUser(model);
            if (user == null)
                return Unauthorized("ERR_BAD_CREDENTIALS");

            if (!user.Enabled)
                return Unauthorized("ERR_ACCOUNT_DISABLED");

            var rsp = _jwtTokenGenerator.GenerateJwtToken(user);
            if (string.IsNullOrEmpty(rsp?.Token))
                return Unauthorized("ERR_NO_TOKEN");

            // If succesfully logged in, register the device used to log in
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            _authHelper.RegisterDevice(deviceId, ipAddress, user.LoginId);

            return Ok(rsp);
        }


        [HttpGet("all")]
        [ProducesResponseType(typeof(User[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            try
            {
                return Ok(_authHelper.AllUsers());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("save")]
        [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "SaveUser")]
        public IActionResult SaveUser([FromBody] User user)
        {
            try
            {
                var dbu = _authHelper.SaveUser(user, out bool inserted);
                if (dbu != null)
                {
                    dbu.PasswordHash = null;

                    return inserted ?
                        StatusCode(StatusCodes.Status201Created, dbu) :
                        Ok(dbu);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest();
        }

        [HttpPost("delete/{loginId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeleteUser")]
        public IActionResult DeleteUser([FromRoute] string loginId)
        {
            try
            {
                return StatusCode(_authHelper.DeleteUser(loginId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Menus), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetMenus")]
        [AllowAnonymous]
        public IActionResult GetMenus([FromHeader(Name = "X-Device-Id")] string deviceId)
        {
            try
            {
                string registeredDeviceId =
                    _authHelper.GetRegisteredDevice(deviceId) == null ?
                        Guid.NewGuid().ToString().Replace("-", "").ToLowerInvariant() :
                        deviceId;

                return Ok(new Menus
                {
                    PublicMenus = _authHelper.PublicMenus(deviceId),
                    AppMenus = _authHelper.ApplicationMenus(deviceId, HttpContext.User.Identity),
                    DeviceId = registeredDeviceId
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
                return NotFound(ex.Message);
            }
        }
    }
}
