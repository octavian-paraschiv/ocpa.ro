using System.Collections.Generic;

namespace ocpa.ro.api.Models.Meteo
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

    public class City
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Default { get; set; }
    }

    public class GridCoordinates
    {
        public int R { get; set; }
        public int C { get; set; }
    }
}