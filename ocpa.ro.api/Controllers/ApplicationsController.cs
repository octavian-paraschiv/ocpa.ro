using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Policies;
using ocpa.ro.api.Swagger;
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
    [ApiExplorerSettings(GroupName = SwaggerConfiguration.AccessManagement)]
    public class ApplicationsController : ApiControllerBase
    {
        private readonly IAccessManagementService _accessManagementService;

        public ApplicationsController(IAccessManagementService accessManagementService, ILogger logger)
            : base(logger)
        {
            _accessManagementService = accessManagementService ?? throw new ArgumentNullException(nameof(accessManagementService));
        }

        //------------------------

        [HttpGet("all")]
        [ProducesResponseType(typeof(ApplicationUser[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetAllApplications")]
        public IActionResult GetAllApplications()
        {
            try
            {
                return Ok(_accessManagementService.GetApplications());
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
        [SwaggerOperation(OperationId = "SaveApplication")]
        public IActionResult SaveApplication([FromBody] Application app)
        {
            try
            {
                var dbu = _accessManagementService.SaveApplication(app, out bool inserted);
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

        [HttpPost("delete/{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeleteApplication")]
        public IActionResult DeleteApplication([FromRoute] int appId)
        {
            try
            {
                return StatusCode(_accessManagementService.DeleteApplication(appId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //------------------------

        [HttpGet("menus")]
        [ProducesResponseType(typeof(ApplicationUser[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "GetApplicationMenus")]
        public IActionResult GetApplicationMenus()
        {
            try
            {
                return Ok(_accessManagementService.GetApplicationMenus());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{appId}/menu/save/{menuId}")]
        [ProducesResponseType(typeof(ApplicationMenu), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApplicationMenu), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "SaveApplicationMenu")]
        public IActionResult SaveApplicationMenu([FromRoute] int appId, [FromRoute] int menuId)
        {
            try
            {
                var dbu = _accessManagementService.SaveApplicationMenu(appId, menuId, out bool inserted);
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

        [HttpPost("{appId}/menu/delete/{menuId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [IgnoreWhenNotInDev]
        [SwaggerOperation(OperationId = "DeleteApplicationMenu")]
        public IActionResult DeleteApplicationMenu([FromRoute] int appId, [FromRoute] int menuId)
        {
            try
            {
                return StatusCode(_accessManagementService.DeleteApplicationMenu(appId, menuId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
