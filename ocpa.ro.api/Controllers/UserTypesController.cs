using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Entities.Application;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("user-types")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ApiExplorerSettings(GroupName = "Users")]
    public class UserTypesController : ApiControllerBase
    {
        private readonly IAccessManagementService _accessManagementService;

        public UserTypesController(IAccessManagementService accessManagementService, ILogger logger)
            : base(logger)
        {
            _accessManagementService = accessManagementService ?? throw new ArgumentNullException(nameof(accessManagementService));
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(UserType[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllUserTypes")]
        public IActionResult GetAllUserTypes()
        {
            try
            {
                return Ok(_accessManagementService.AllUserTypes());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
