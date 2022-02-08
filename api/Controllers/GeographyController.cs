using common;
using Meteo.Helpers;
using OPMedia.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;

namespace OPMedia.API.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class GeographyController : ApiController
    {
        static string _configFilePath = null;
        static FileSystemWatcher _watchConfigFile = null;
        static List<Region> _regions = new List<Region>();


        static GeographyController()
        {
            _configFilePath = HostingEnvironment.MapPath($"/meteo/data/GeographicData.json");

            ReadConfigFile();

            _watchConfigFile = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_configFilePath),
                Filter = Path.GetFileName(_configFilePath),
                EnableRaisingEvents = true
            };

            _watchConfigFile.Changed += (s, e) =>
            {
                try
                {
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                        case WatcherChangeTypes.Changed:
                            ReadConfigFile();
                            break;
                    }
                }
                catch
                {
                }
            };
        }

        static void ReadConfigFile()
        {
            var json = File.ReadAllText(_configFilePath);
            _regions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Region>>(json);
        }

        public List<string> GetRegionNames()
        {
            return (from rgn in _regions
                    orderby rgn.Name
                    select rgn.Name).Distinct().ToList();
        }

        public List<string> GetSubregionNames(string region)
        {
            _ = GetRegion(region);

            return (from rgn in _regions
                    from city in rgn.Cities
                    where rgn.Name == region
                    orderby city.Subregion
                    select city.Subregion).Distinct().ToList();
        }

        public List<string> GetCityNames(string region, string subregion)
        {
            ValidateSubregion(region, subregion);

            return (from rgn in _regions
                    from city in rgn.Cities
                    where rgn.Name == region && city.Subregion == subregion
                    orderby city.Default descending, city.Name
                    select city.Name).Distinct().ToList();
        }

        public GridCoordinates GetGridCoordinates(string region, string subregion, string city)
        {
            var rObj = GetRegion(region);
            var cObj = GetCity(region, subregion, city);

            if (rObj.MinLat >= cObj.Latitude || cObj.Latitude >= rObj.MaxLat)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent($"City {city} has latitude outside region {region}")
                });

            if (rObj.MinLon >= cObj.Longitude || cObj.Longitude >= rObj.MaxLon)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent($"City {city} has longitude outside region {region}")
                });

            int gridWidth = 1 + (int)((rObj.MaxLon - rObj.MinLon) / rObj.GridResolution);
            int gridHeight = 1 + (int)((rObj.MaxLat - rObj.MinLat) / rObj.GridResolution);

            int dw = (int)(gridWidth * (cObj.Longitude - rObj.MinLon) / (rObj.MaxLon - rObj.MinLon));
            int dh = (int)(gridHeight * (rObj.MaxLat - cObj.Latitude) / (rObj.MaxLat - rObj.MinLat));

            return new GridCoordinates { C = dw, R = dh };
        }

        private Region GetRegion(string region)
        {
            var rObj = (from rgn in _regions
                        where rgn.Name == region
                        select rgn).FirstOrDefault();

            if (rObj == null)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent($"Could not find any region named {region}")
                });

            return rObj;
        }

        private void ValidateSubregion(string region, string subregion)
        {
            _ = GetRegion(region);

            var x = (from rgn in _regions
                     from c in rgn.Cities
                     where rgn.Name == region && c.Subregion == subregion
                     select 1).Count();

            if (x < 1)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent($"Couuld not find any subregion named {subregion} in region {region}")
                });
        }

        private City GetCity(string region, string subregion, string city)
        {
            ValidateSubregion(region, subregion);

            var cObj = (from rgn in _regions
                        from c in rgn.Cities
                        where rgn.Name == region && c.Subregion == subregion && c.Name == city
                        select c).FirstOrDefault();

            if (cObj == null)
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent($"Couuld not find any city named {city} in region {region} subregion {subregion}")
                });

            return cObj;
        }
    }
}
