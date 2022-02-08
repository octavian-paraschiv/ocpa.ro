using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPMedia.Backend.Models
{
    public class City
    {
        public string Name { get; set; }
        public string Subregion { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Default { get; set; }
    }
}