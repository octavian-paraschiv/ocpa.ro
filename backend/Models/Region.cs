using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThorusCommon.IO;

namespace OPMedia.Backend.Models
{
    public class Region
    {
        public string Name { get; set; }

        public double MinLon { get; set; }
        public double MaxLon { get; set; }

        public double MinLat { get; set; }
        public double MaxLat { get; set; }

        public double GridResolution { get; set; }

        public List<City> Cities { get; set; }

    }
}