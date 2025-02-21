using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Helpers.Email;
using ocpa.ro.api.Models.Applications;
using ocpa.ro.api.Models.Authentication;
using ocpa.ro.api.Models.Configuration;
using ocpa.ro.api.Models.Menus;
using ocpa.ro.api.Policies;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

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
        private readonly IEmailHelper _emailHelper;
        private readonly IJwtTokenHelper _jwtTokenGenerator;
        private readonly AuthConfig _config;

        public UsersController(IWebHostEnvironment hostingEnvironment,
            IAuthHelper authHelper,
            IJwtTokenHelper jwtTokenGenerator,
            ILogger logger,
            IEmailHelper emailHelper,
            IOptions<AuthConfig> config)
            : base(hostingEnvironment, logger, authHelper)
        {
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _emailHelper = emailHelper ?? throw new ArgumentNullException(nameof(emailHelper));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpPost("mfa")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FailedAuthenticationResponse), StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = "MfaChallenge")]
        public IActionResult MfaChallenge([FromForm] AuthenticateRequest model)
        {
            var user = _authHelper.MfaChallenge(model);
            if (user == null)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS"));

            if (!user.Enabled)
                return Unauthorized(new FailedAuthenticationResponse("ERR_ACCOUNT_DISABLED"));

            if (user.LoginAttemptsRemaining < _config.MaxLoginRetries)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_MFA_CHALLENGE", user.LoginAttemptsRemaining));

            var rsp = _jwtTokenGenerator.GenerateJwtToken(user);
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
        public IActionResult Authenticate([FromForm] AuthenticateRequest model,
            [FromHeader(Name = "X-Refresh-Auth")] string refreshAuth,
            [FromHeader(Name = "X-Language")] string language,
            [FromHeader(Name = "X-Device-Id")] string deviceId)
        {
            bool isRefreshAuth = refreshAuth == "1";

            var user = _authHelper.AuthorizeUser(model);
            if (user == null)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS"));

            if (!user.Enabled)
                return Unauthorized(new FailedAuthenticationResponse("ERR_ACCOUNT_DISABLED"));

            if (user.LoginAttemptsRemaining < _config.MaxLoginRetries)
                return Unauthorized(new FailedAuthenticationResponse("ERR_BAD_CREDENTIALS", user.LoginAttemptsRemaining));


            if (isRefreshAuth)
            {
                var rsp = _jwtTokenGenerator.GenerateJwtToken(user);
                if (string.IsNullOrEmpty(rsp?.Token))
                    return Unauthorized(new FailedAuthenticationResponse("ERR_NO_TOKEN", user.LoginAttemptsRemaining));

                return Ok(rsp);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    // If succesfully logged in, register the device used to log in
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _authHelper.RegisterDevice(deviceId, ipAddress, user.LoginId);
                });

                if (user.MfaChallenge?.Length > 0)
                {
                    ThreadPool.QueueUserWorkItem(async _ =>
                    {
                        try
                        {
                            var ci = new CultureInfo(language);
                            language = ci.TwoLetterISOLanguageName.ToLowerInvariant();
                        }
                        catch
                        {
                            language = "en";
                        }

                        try
                        {
                            await _emailHelper.SendMfaChallenge(user.EmailAddress, user.MfaChallenge, language);
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    });

                    var rsp = new AuthenticationResponse
                    {
                        LoginId = user.LoginId,
                        Type = user?.Type ?? default,
                        UseMFA = true,
                        Token = null
                    };

                    return Ok(rsp);
                }
                else
                {
                    var rsp = _jwtTokenGenerator.GenerateJwtToken(user);
                    if (string.IsNullOrEmpty(rsp?.Token))
                        return Unauthorized(new FailedAuthenticationResponse("ERR_NO_TOKEN", user.LoginAttemptsRemaining));

                    return Ok(rsp);
                }
            }
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
                    dbu.MfaChallenge = null;

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
                return Ok(_authHelper.GetAppsForUser(userId));
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
                _authHelper.SaveAppsForUser(userId, appsForUser);
                return Ok();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{userId}/apps/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeleteAppsForUser")]
        public IActionResult DeleteAppsForUser([FromRoute] int userId)
        {
            try
            {
                _authHelper.DeleteAppsForUser(userId, false);
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
