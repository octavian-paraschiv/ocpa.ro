using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;

namespace ocpa.ro.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SensorsController : ApiControllerBase
    {
        private const int count = 5;
        private static readonly Random rnd = new Random();

        public SensorsController(IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
        }

        [HttpGet("sensors")]
        public SensorDataCollection ReadSensors()
        {
            SensorDataCollection sensorDataCollection = new SensorDataCollection
            {
                SensorData = new List<SensorData>()
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
            return sensorDataCollection;
        }
    }
}
