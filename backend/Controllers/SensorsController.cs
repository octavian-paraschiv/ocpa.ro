using OPMedia.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace OPMedia.Backend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SensorsController : ApiController
    {
        const int count = 5;
        static readonly Random rnd = new Random();

        [HttpGet]
        public SensorDataCollection ReadSensors()
        {
            SensorDataCollection col = new SensorDataCollection { SensorData = new List<SensorData>() };

            for (int i = 0; i < count; i++)
            {
                col.SensorData.Add(new SensorData
                {
                    Temperature = rnd.Next(15, 50),
                    Humidity = rnd.Next(80, 120),
                    Pressure = rnd.Next(25, 75)
                });
            }

            return col;
        }
    }
}