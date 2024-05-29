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
    public class UsersController : ApiControllerBase
    {
        private readonly IJwtTokenHelper _jwtTokenGenerator;

        public UsersController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, IJwtTokenHelper jwtTokenGenerator)
            : base(hostingEnvironment, authHelper)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
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

        [Authorize(Roles = "Admin")]
        [HttpPost("save")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult SaveUser([FromBody] User user)
        {
            try
            {
                var dbu = _authHelper.SaveUser(user);
                if (dbu != null)
                {
                    dbu.PasswordHash = null;
                    return Ok(dbu);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest();
        }
    }
}
