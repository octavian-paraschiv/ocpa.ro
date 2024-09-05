using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Medical;
using ocpa.ro.api.Models.Generic;
using ocpa.ro.api.Models.Medical.Database;
using ocpa.ro.api.Policies;
using Serilog;
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
        private readonly IMedicalDataHelper _dataHelper = null;

        public MedicalController(IWebHostEnvironment hostingEnvironment, IMedicalDataHelper dbHelper, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            _dataHelper = dbHelper;
        }

        [HttpGet("test-types")]
        [ProducesResponseType(typeof(List<TestTypeDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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

        [HttpGet("person")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetPerson([FromQuery] string loginId)
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

        [HttpGet("tests")]
        [ProducesResponseType(typeof(List<TestDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetTests([FromQuery] int? id, [FromQuery] int? pid, [FromQuery] string cnp,
            [FromQuery] string category, [FromQuery] string type,
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                return Ok(_dataHelper.Tests(id, pid, cnp, category, type, from, to));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("tests")]
        public IActionResult SaveTest([FromBody] Test r) => SaveMedicalRecord(r);

        [HttpPost("test-types")]
        [Authorize(Roles = "ADM")]
        public IActionResult SaveTestType([FromBody] TestType r) => SaveMedicalRecord(r);

        [HttpPost("test-categories")]
        [Authorize(Roles = "ADM")]
        public IActionResult SaveTestCategory([FromBody] TestCategory r) => SaveMedicalRecord(r);

        [HttpPost("labs")]
        [Authorize(Roles = "ADM")]
        public IActionResult SaveLab([FromBody] Lab r) => SaveMedicalRecord(r);


        [HttpPost("delete/test/{id}")]
        public IActionResult DeleteTest([FromRoute] int id) => DeleteMedicalRecord<Test>(id);

        [HttpPost("delete/test-type/{id}")]
        [Authorize(Roles = "ADM")]
        public IActionResult DeleteTestType([FromRoute] int id) => DeleteMedicalRecord<TestType>(id);

        [HttpPost("delete/test-category/{id}")]
        [Authorize(Roles = "ADM")]
        public IActionResult DeleteTestCategory([FromRoute] int id) => DeleteMedicalRecord<TestCategory>(id);

        [HttpPost("delete/lab/{id}")]
        [Authorize(Roles = "ADM")]
        public IActionResult DeleteLab([FromRoute] int id) => DeleteMedicalRecord<Lab>(id);

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
    }
}
