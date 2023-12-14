using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers;
using ocpa.ro.api.Models;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UsersController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, IJwtTokenGenerator jwtTokenGenerator)
            : base(hostingEnvironment, authHelper)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromForm] AuthenticateRequest model)
        {
            var user = _authHelper.AuthorizeUser(model);
            if (user == null)
                return Unauthorized();

            var token = _jwtTokenGenerator.GenerateJwtToken(user);
            if (token == null)
                return Unauthorized();

            var rsp = new AuthenticateResponse
            {
                LoginId = user.LoginId,
                Token = token
            };

            return Ok(rsp);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("save")]
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
