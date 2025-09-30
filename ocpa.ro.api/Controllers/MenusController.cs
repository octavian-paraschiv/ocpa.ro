using Microsoft.AspNetCore.Authorization;
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
    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "ADM")]
    [ApiExplorerSettings(GroupName = "Menus")]
    public class MenusController : ApiControllerBase
    {
        private readonly IAccessManagementService _accessManagementService;

        public MenusController(IAccessManagementService accessManagementService, ILogger logger)
            : base(logger)
        {
            _accessManagementService = accessManagementService ?? throw new ArgumentNullException(nameof(accessManagementService));
        }

        //------------------------

        [HttpGet("all")]
        [ProducesResponseType(typeof(Menu[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllMenus")]
        public IActionResult GetAllMenus()
        {
            try
            {
                return Ok(_accessManagementService.GetMenus());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("save")]
        [ProducesResponseType(typeof(ApplicationUser), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApplicationUser), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "SaveMenu")]
        public IActionResult SaveMenu([FromBody] Menu menu)
        {
            try
            {
                var dbu = _accessManagementService.SaveMenu(menu, out bool inserted);
                if (dbu != null)
                    return inserted ?
                        StatusCode(StatusCodes.Status201Created, dbu) :
                        Ok(dbu);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest();
        }

        [HttpPost("delete/{menuId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeleteMenu")]
        public IActionResult DeleteMenu([FromRoute] int menuId)
        {
            try
            {
                return StatusCode(_accessManagementService.DeleteMenu(menuId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
