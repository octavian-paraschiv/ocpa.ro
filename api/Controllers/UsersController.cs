using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Models.Authentication;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ApiControllerBase
    {
        private readonly IJwtTokenHelper _jwtTokenGenerator;

        public UsersController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, IJwtTokenHelper jwtTokenGenerator)
            : base(hostingEnvironment, authHelper)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("authenticate")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        public IActionResult Authenticate([FromForm] AuthenticateRequest model)
        {
            var user = _authHelper.AuthorizeUser(model);
            if (user == null)
                return Unauthorized();

            var rsp = _jwtTokenGenerator.GenerateJwtToken(user);
            if (string.IsNullOrEmpty(rsp?.Token))
                return Unauthorized();

            return Ok(rsp);
        }


        [HttpGet]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
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

        [HttpDelete("delete/{loginId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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
    }
}
