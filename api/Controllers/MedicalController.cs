using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Medical;
using ocpa.ro.api.Models.Generic;
using ocpa.ro.api.Models.Medical;
using ocpa.ro.api.Policies;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "ADM, APP")]
    [IgnoreWhenNotInDev]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MedicalController : ApiControllerBase
    {
        #region Private members
        private readonly IMedicalDataHelper _dataHelper = null;
        #endregion

        #region Constructor (DI)
        public MedicalController(IWebHostEnvironment hostingEnvironment, IMedicalDataHelper dbHelper, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            _dataHelper = dbHelper;
        }
        #endregion

        #region Public controller methods
        [HttpGet("test-types")]
        [ProducesResponseType(typeof(List<TestTypeDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetTestTypes")]
        public IActionResult GetTestTypes([FromQuery] string category)
        {
            try
            {
                return Ok(_dataHelper.TestTypes(category));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("labs")]
        [ProducesResponseType(typeof(List<Lab>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetLabs")]
        public IActionResult GetLabs()
        {
            try
            {
                return Ok(_dataHelper.AllOfType<Lab>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test-categories")]
        [ProducesResponseType(typeof(List<TestCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetTestCategories")]
        public IActionResult GetTestCategories()
        {
            try
            {
                return Ok(_dataHelper.AllOfType<TestCategory>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("person/{loginId}")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "GetPerson")]
        public IActionResult GetPerson([FromRoute] string loginId)
        {
            try
            {
                var person = _dataHelper.Person(loginId);
                try
                {
                    var rp = new RomanianPerson(person);
                    person = rp;
                }
                catch
                {
                    // Not a Romanian resident so strip off CNP
                    person.CNP = null;
                }

                return Ok(person);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST because this method takes PII data
        [HttpPost("search/tests")]
        [ProducesResponseType(typeof(List<TestDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(OperationId = "SearchTests")]
        public IActionResult SearchTests([FromBody] TestSearchRequest request)
        {
            try
            {
                return Ok(_dataHelper.SearchTests(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("save/test")]
        [SwaggerOperation(OperationId = "SaveTest")]
        public IActionResult SaveTest([FromBody] Test r) => SaveMedicalRecord(r);

        [HttpPost("save/test-type")]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "SaveTestType")]
        public IActionResult SaveTestType([FromBody] TestType r) => SaveMedicalRecord(r);

        [HttpPost("save/test-category")]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "SaveTestCategory")]
        public IActionResult SaveTestCategory([FromBody] TestCategory r) => SaveMedicalRecord(r);

        [HttpPost("save/lab")]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "SaveLab")]
        public IActionResult SaveLab([FromBody] Lab r) => SaveMedicalRecord(r);


        [HttpPost("delete/test/{id}")]
        [SwaggerOperation(OperationId = "DeleteTest")]
        public IActionResult DeleteTest([FromRoute] int id) => DeleteMedicalRecord<Test>(id);

        [HttpPost("delete/test-type/{id}")]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "DeleteTestType")]
        public IActionResult DeleteTestType([FromRoute] int id) => DeleteMedicalRecord<TestType>(id);

        [HttpPost("delete/test-category/{id}")]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "DeleteTestCategory")]
        public IActionResult DeleteTestCategory([FromRoute] int id) => DeleteMedicalRecord<TestCategory>(id);

        [HttpPost("delete/lab/{id}")]
        [Authorize(Roles = "ADM")]
        [SwaggerOperation(OperationId = "DeleteLab")]
        public IActionResult DeleteLab([FromRoute] int id) => DeleteMedicalRecord<Lab>(id);
        #endregion

        #region Private methods
        private IActionResult SaveMedicalRecord<T>(T t) where T : class, IMedicalDbTable, new()
        {
            try
            {
                return StatusCode(_dataHelper.SaveMedicalRecord(t));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private IActionResult DeleteMedicalRecord<T>(int id) where T : class, IMedicalDbTable, new()
        {
            try
            {
                return StatusCode(_dataHelper.DeleteMedicalRecord<T>(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
