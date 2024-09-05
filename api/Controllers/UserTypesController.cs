using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Models.Authentication;
using ocpa.ro.api.Policies;
using Serilog;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("user-types")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class UserTypesController : ApiControllerBase
    {
        public UserTypesController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, ILogger logger)
            : base(hostingEnvironment, logger, authHelper)
        {
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(UserType[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        public IActionResult GetAllUserTypes()
        {
            try
            {
                return Ok(_authHelper.AllUserTypes());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
