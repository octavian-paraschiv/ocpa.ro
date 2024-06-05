using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Medical;
using ocpa.ro.api.Models.Generic;
using ocpa.ro.api.Models.Medical.Database;
using System;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Patient")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesErrorResponseType(typeof(void))]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MedicalController : ApiControllerBase
    {
        private readonly IMedicalDataHelper _dataHelper = null;

        public MedicalController(IWebHostEnvironment hostingEnvironment, IMedicalDataHelper dbHelper)
            : base(hostingEnvironment)
        {
            _dataHelper = dbHelper;
        }

        [HttpGet("testTypes")]
        public IActionResult GetContentPath([FromQuery] string category)
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
        public IActionResult GetLabs()
        {
            try
            {
                return Ok(_dataHelper.UnfilteredTable<Lab>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("testCategories")]
        public IActionResult GetTestCategories()
        {
            try
            {
                return Ok(_dataHelper.UnfilteredTable<TestCategory>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("person")]
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

        [HttpPost("testTypes")]
        [Authorize(Roles = "Admin")]
        public IActionResult SaveTestType([FromBody] TestType r) => SaveMedicalRecord(r);

        [HttpPost("testCategories")]
        [Authorize(Roles = "Admin")]
        public IActionResult SaveTestCategory([FromBody] TestCategory r) => SaveMedicalRecord(r);

        [HttpPost("labs")]
        [Authorize(Roles = "Admin")]
        public IActionResult SaveLab([FromBody] Lab r) => SaveMedicalRecord(r);


        [HttpDelete("tests")]
        public IActionResult DeleteTest([FromBody] Test r) => DeleteMedicalRecord(r);

        [HttpDelete("testTypes")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteTestType([FromBody] TestType r) => DeleteMedicalRecord(r);

        [HttpDelete("testCategories")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteTestCategory([FromBody] TestCategory r) => DeleteMedicalRecord(r);

        [HttpDelete("labs")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteLab([FromBody] Lab r) => DeleteMedicalRecord(r);

        private IActionResult SaveMedicalRecord<T>(T t) where T : IMedicalDbTable, new()
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

        private IActionResult DeleteMedicalRecord<T>(T t) where T : IMedicalDbTable, new()
        {
            try
            {
                return StatusCode(_dataHelper.DeleteMedicalRecord(t));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
