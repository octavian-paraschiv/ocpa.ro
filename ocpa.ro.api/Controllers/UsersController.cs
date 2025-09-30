using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Extensions;
using ocpa.ro.domain.Models.Authentication;
using ocpa.ro.domain.Models.Configuration;
using ocpa.ro.domain.Models.Menus;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "ADM")]
    [ApiExplorerSettings(GroupName = "Users")]
    public class UsersController : ApiControllerBase
    {
        private readonly IOneTimePasswordService _oneTimePasswordService;
        private readonly IAccessService _accessService;
        private readonly IAccessManagementService _accessManagementService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly AuthConfig _config;

        public UsersController(IWebHostEnvironment hostingEnvironment,
            IOneTimePasswordService oneTimePasswordService,
            IAccessService accessService,
            IAccessTokenService accessTokenService,
            IAccessManagementService accessManagementService,
            ILogger logger,
            ISystemSettingsService settingsService)
            : base(logger)
        {
            _oneTimePasswordService = oneTimePasswordService ?? throw new ArgumentNullException(nameof(oneTimePasswordService));
            _accessService = accessService ?? throw new ArgumentNullException(nameof(accessService));
            _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
            _accessManagementService = accessManagementService ?? throw new ArgumentNullException(nameof(accessManagementService));
            _config = settingsService.AuthenticationSettings;
        }

        [HttpPost("validate-otp")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FailedAuthenticationResponse), StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "ValidateOTP")]
        public IActionResult ValidateOTP([FromForm] AuthenticateRequest model)
        {
            var (err, user) = _oneTimePasswordService.ValidateOneTimePassword(model);
            if (err?.Length > 0 || user == null)
                return Unauthorized(new FailedAuthenticationResponse(err));

            var rsp = _accessTokenService.GenerateAccessToken(user);
            if (string.IsNullOrEmpty(rsp?.Token))
                return Unauthorized(new FailedAuthenticationResponse("ERR_NO_TOKEN", user.LoginAttemptsRemaining));

            return Ok(rsp);
        }

        [HttpPost("generate-otp")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FailedAuthenticationResponse), StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "GenerateOTP")]
        public async Task<IActionResult> GenerateOTP([FromQuery] string loginId)
        {
            try
            {
                bool success = await _oneTimePasswordService.GenerateOneTimePassword(loginId, RequestLanguage);
                return Ok(success);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FailedAuthenticationResponse), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(OperationId = "RefreshToken")]
        public IActionResult RefreshToken([FromQuery] string loginId)
        {
            var bearerToken = (Request?.Headers?.Authorization ?? "").ToString().Replace("Bearer", "").Trim();
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(bearerToken);
            var claimedID = jwtToken?.Claims?.Where(c => c.Type == "id")?.Select(c => c.Value)?.FirstOrDefault();
            var now = DateTime.UtcNow;

            if (jwtToken == null ||
                jwtToken.ValidFrom >= now ||
                jwtToken.ValidTo <= now ||
                claimedID != loginId)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS"));

            var user = _accessService.GetUser(loginId);
            if (user == null)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS"));

            var rsp = _accessTokenService.GenerateAccessToken(user);
            if (string.IsNullOrEmpty(rsp?.Token))
                return Unauthorized(new FailedAuthenticationResponse("ERR_NO_TOKEN", user.LoginAttemptsRemaining));

            return Ok(rsp);
        }

        [HttpPost("authenticate")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FailedAuthenticationResponse), StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "Authenticate")]
        public async Task<IActionResult> Authenticate([FromForm] AuthenticateRequest model,
            [FromHeader(Name = "X-Device-Id")] string deviceId)
        {
            var (user, useOTP) = _accessService.Authenticate(model);
            if (user == null)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS"));

            if (!user.Enabled)
                return Unauthorized(new FailedAuthenticationResponse("ERR_ACCOUNT_DISABLED"));

            if (user.LoginAttemptsRemaining < _config.MaxLoginRetries)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS", user.LoginAttemptsRemaining));

            AuthenticationResponse rsp = null;

            if (useOTP)
            {
                rsp = new AuthenticationResponse
                {
                    LoginId = user.LoginId,
                    Type = user?.Type ?? default,
                    SendOTP = useOTP,
                    AnonymizedEmail = StringUtility.AnonymizeEmail(user?.EmailAddress ?? ""),
                    Token = null
                };
            }
            else
            {
                rsp = _accessTokenService.GenerateAccessToken(user);
                rsp.SendOTP = false;

                if (string.IsNullOrEmpty(rsp?.Token))
                    return Unauthorized(new FailedAuthenticationResponse("ERR_NO_TOKEN", user.LoginAttemptsRemaining));
            }

            try
            {
                // If succesfully logged in, register the device used to log in
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _accessService.RegisterDevice(deviceId, ipAddress, user.LoginId);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

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
                return Ok(_accessService.AllUsers());
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
                var dbu = _accessService.SaveUser(user, out bool inserted);
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
                return StatusCode(_accessService.DeleteUser(loginId));
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
                    _accessService.GetRegisteredDevice(deviceId) == null ?
                        Guid.NewGuid().ToString().Replace("-", "").ToLowerInvariant() :
                        deviceId;

                return Ok(new Menus
                {
                    PublicMenus = _accessService.PublicMenus(deviceId),
                    AppMenus = _accessService.ApplicationMenus(deviceId, HttpContext.User.Identity),
                    DeviceId = registeredDeviceId
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
                return NotFound(ex.Message);
            }
        }


        [HttpGet("{userId}/apps")]
        [ProducesResponseType(typeof(ApplicationUser[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAppsForUser")]
        public IActionResult GetAppsForUser([FromRoute] int userId)
        {
            try
            {
                return Ok(_accessManagementService.GetAppsForUser(userId));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{userId}/apps/save")]
        [ProducesResponseType(typeof(ApplicationUser), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApplicationUser), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "SaveAppsForUser")]
        public IActionResult SaveAppsForUser([FromRoute] int userId, [FromBody] IEnumerable<ApplicationUser> appsForUser)
        {
            try
            {
                _accessManagementService.SaveAppsForUser(userId, appsForUser);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
