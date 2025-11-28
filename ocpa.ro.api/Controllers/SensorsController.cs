using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Swagger;
using ocpa.ro.domain.Models.Sensors;
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
    [ApiExplorerSettings(GroupName = SwaggerConfiguration.Experimental)]
    public class SensorsController : ApiControllerBase
    {
        private const int count = 5;
        private static readonly Random rnd = new();

        public SensorsController(ILogger logger)
            : base(logger)
        {
        }

        [HttpGet("data")]
        [ProducesResponseType(typeof(SensorDataCollection), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = "ReadSensors")]
        public IActionResult ReadSensors()
        {
            SensorDataCollection sensorDataCollection = new()
            {
                SensorData = []
            };
            for (int i = 0; i < count; i++)
            {
                sensorDataCollection.SensorData.Add(new SensorData
                {
                    Temperature = rnd.Next(15, 50),
                    Humidity = rnd.Next(80, 120),
                    Pressure = rnd.Next(25, 75)
                });
            }
            return Ok(sensorDataCollection);
        }
    }
}
