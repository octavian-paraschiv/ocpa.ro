using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Exceptions;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Geography;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Helpers.Geography;

public interface IGeographyHelper
{
    string FirstRegionCode { get; }
    IEnumerable<string> GetRegionCodes();
    IEnumerable<string> GetRegionNames();
    IEnumerable<string> GetSubregionNames(string regionName);
    IEnumerable<string> GetCityNames(string regionName, string subregionName);
    IEnumerable<CityDetail> GetAllCities();
    Region GetRegion(string regionName);
    void ValidateSubregion(string regionName, string subregionName);
    CityDetail GetCity(string regionName, string subregionName, string cityName);
    GridCoordinates GetGridCoordinates(string regionName, string subregionName, string cityName);
    Task<GeoLocation> GetGeoLocation(string ipAddress);
}

public class GeographyHelper : BaseHelper, IGeographyHelper
{
    #region Private members
    private readonly GeographyDB _db;
    private readonly HttpClient _client;
    #endregion

    #region Constructor (DI)
    public GeographyHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IHttpClientFactory factory)
       : base(hostingEnvironment, logger)
    {
        var dataFolder = Path.Combine(hostingEnvironment.ContentPath(), "Geography");
        var dbPath = Path.Combine(dataFolder, "Geography.db3");
        _db = new GeographyDB(dbPath, false);

        _client = factory.CreateClient("geolocation");
    }
    #endregion

    #region IGeographyHelper implementation
    public string FirstRegionCode => GetRegionCodes().FirstOrDefault()?.ToUpper();

    public IEnumerable<string> GetRegionCodes()
        => _db.Regions.AsEnumerable().Select(r => r.Code);

    public IEnumerable<string> GetRegionNames()
        => _db.Regions.AsEnumerable().Select(r => r.Name);

    public IEnumerable<string> GetSubregionNames(string regionName)
    {
        var region = GetRegion(regionName);

        return _db.Cities
            .Where(c => c.RegionId == region.Id)
            .OrderBy(c => c.Subregion)
            .AsEnumerable()
            .Select(c => c.Subregion)
            .Distinct();
    }

    public IEnumerable<string> GetCityNames(string regionName, string subregionName)
    {
        var region = GetRegion(regionName);
        ValidateSubregion(regionName, subregionName);

        return _db.Cities
            .Where(c => c.RegionId == region.Id && c.Subregion.ToUpper() == subregionName.ToUpper())
            .OrderByDescending(c => c.Default)
            .ThenBy(c => c.Name)
            .AsEnumerable()
            .Select(c => c.Name)
            .Distinct();
    }

    public IEnumerable<CityDetail> GetAllCities()
    {
        return _db.Cities
            .OrderBy(c => c.Name)
            .AsEnumerable()
            .Select(c => new CityDetail
            {
                Default = c.Default,
                Id = c.Id,
                Lat = c.Lat,
                Lon = c.Lon,
                Name = c.Name,
                RegionName = GetRegion(c.RegionId).Name,
                RegionCode = GetRegion(c.RegionId).Code,
                Subregion = c.Subregion,
                RegionId = c.RegionId,
            });

    }

    public CityDetail GetCity(string regionName, string subregionName, string cityName)
    {
        var region = GetRegion(regionName);
        ValidateSubregion(regionName, subregionName);

        var city = _db.Cities
            .Where(c => c.RegionId == region.Id && c.Subregion.ToUpper() == subregionName.ToUpper() && c.Name.ToUpper() == cityName.ToUpper())
            .Select(c => new CityDetail
            {
                Default = c.Default,
                Id = c.Id,
                Lat = c.Lat,
                Lon = c.Lon,
                Name = c.Name,
                RegionName = GetRegion(c.RegionId).Name,
                RegionCode = GetRegion(c.RegionId).Code,
                Subregion = c.Subregion,
                RegionId = c.RegionId,
            })
            .FirstOrDefault();

        return city ??
            throw new ExtendedException($"Could not find any city named '{cityName}' " +
                $"in region '{regionName}', subregion '{subregionName}'");
    }

    public Region GetRegion(string regionName)
    {
        regionName ??= "";
        Region region2 = _db.Regions.Where(rgn => rgn.Name.ToUpper() == regionName.ToUpper()).FirstOrDefault();
        return region2 ?? throw new ExtendedException($"Could not find any region named '{regionName}'");
    }

    public GridCoordinates GetGridCoordinates(string regionName, string subregionName, string cityName)
    {
        Region region2 = GetRegion(regionName);
        City city2 = GetCity(regionName, subregionName, cityName);

        if (region2.MinLat >= city2.Lat || city2.Lat >= region2.MaxLat)
            throw new ExtendedException($"City '{cityName}' has latitude outside region '{regionName}'");

        if (region2.MinLon >= city2.Lon || city2.Lon >= region2.MaxLon)
            throw new ExtendedException($"City '{cityName}' has longitude outside region '{regionName}'");

        int num = 1 + (int)((region2.MaxLon - region2.MinLon) / region2.GridResolution);
        int num2 = 1 + (int)((region2.MaxLat - region2.MinLat) / region2.GridResolution);
        int c = (int)(num * (city2.Lon - region2.MinLon) / (region2.MaxLon - region2.MinLon));
        int r = (int)(num2 * (region2.MaxLat - city2.Lat) / (region2.MaxLat - region2.MinLat));

        return new GridCoordinates
        {
            C = c,
            R = r
        };
    }

    public void ValidateSubregion(string regionName, string subregionName)
    {
        bool found = GetSubregionNames(regionName).Contains(subregionName, StringComparer.OrdinalIgnoreCase);
        if (!found)
            throw new ExtendedException($"Could not find any subregion named '{subregionName}' in region '{regionName}'");
    }

    public async Task<GeoLocation> GetGeoLocation(string ipAddress)
    {
        try
        {
            return await _client.GetFromJsonAsync<GeoLocation>($"{ipAddress}?fields=66846719");
        }
        catch (Exception ex)
        {
            LogException(ex);
            return null;
        }
    }

    #endregion

    private Region GetRegion(int regionId)
        => _db.Regions.Where(r => r.Id == regionId).FirstOrDefault();
}
