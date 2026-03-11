using Microsoft.AspNetCore.Http;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Abstractions.Gateways;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Constants;
using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Exceptions;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCS = ThorusCommon.SQLite;

namespace ocpa.ro.application.Services;

public class GeographyService : BaseService, IGeographyService
{
    #region Private members
    private readonly IApplicationDbContext _dbContext;
    private readonly IGeoLocationGateway _geoLocationGateway;

    #endregion

    #region Constructor (DI)
    public GeographyService(IHostingEnvironmentService hostingEnvironment,
        ILogger logger,
        IGeoLocationGateway geoLocationGateway,
        IApplicationDbContext dbContext)
       : base(hostingEnvironment, logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _geoLocationGateway = geoLocationGateway ?? throw new ArgumentNullException(nameof(geoLocationGateway));

    }
    #endregion

    #region IGeographyHelper implementation
    public Region FirstRegion => _dbContext.Regions.FirstOrDefault();

    public IEnumerable<RegionDetail> GetAllRegions()
    {
        var regions = _dbContext.Regions.Select(r => new RegionDetail
        {
            Code = r.Code,
            GridResolution = r.GridResolution,
            Id = r.Id,
            MaxLat = r.MaxLat,
            MaxLon = r.MaxLon,
            MinLat = r.MinLat,
            MinLon = r.MinLon,
            Name = r.Name,

        }).ToList();

        regions.ForEach(r => r.Subregions = GetSubregionNames(r.Name));

        return regions;
    }

    public IEnumerable<string> GetRegionCodes()
        => [.. _dbContext.Regions.Select(r => r.Code)];

    public IEnumerable<string> GetRegionNames()
        => [.. _dbContext.Regions.Select(r => r.Name)];

    public IEnumerable<string> GetSubregionNames(string regionName)
    {
        var region = GetRegionByName(regionName);

        return [.._dbContext.Cities
            .Where(c => c.RegionId == region.Id)
            .Select(c => c.Subregion)
            .Distinct()
            .OrderBy(c => c)];
    }

    public IEnumerable<string> GetCityNames(string regionName, string subregionName)
    {
        var region = GetRegionByName(regionName);
        ValidateSubregion(regionName, subregionName);

        return [.. _dbContext.Cities
            .Where(c => c.RegionId == region.Id && c.Subregion.ToUpper() == subregionName.ToUpper())
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.Name)
            .Select(c => c.Name)];
    }

    public IEnumerable<CityDetail> GetAllCities()
    {
        var cities = _dbContext.Cities
            .OrderBy(c => c.Name)
            .ToList();

        return cities.Select(GetCityDetail).ToList();

    }

    public CityDetail GetCity(string regionName, string subregionName, string cityName)
    {
        var region = GetRegionByName(regionName);
        ValidateSubregion(regionName, subregionName);

        var city = _dbContext.Cities
            .FirstOrDefault(c => c.RegionId == region.Id && c.Subregion.ToUpper() == subregionName.ToUpper() && c.Name.ToUpper() == cityName.ToUpper());

        if (city == null)
            throw new ExtendedException($"Could not find any city named '{cityName}' " +
                $"in region '{regionName}', subregion '{subregionName}'");

        return GetCityDetail(city);
    }

    public Region GetRegionByName(string regionName)
    {
        regionName ??= "";
        Region region2 = _dbContext.Regions.FirstOrDefault(rgn => rgn.Name.ToUpper() == regionName.ToUpper());
        return region2 ?? throw new ExtendedException($"Could not find any region named '{regionName}'");
    }

    public Region GetRegionByCode(string regionCode)
    {
        regionCode ??= "";
        Region region2 = _dbContext.Regions.FirstOrDefault(rgn => rgn.Code.ToUpper() == regionCode.ToUpper());
        return region2 ?? throw new ExtendedException($"Could not find any region woth code '{regionCode}'");
    }

    public TCS.GridCoordinates GetGridCoordinates(string regionName, string subregionName, string cityName)
    {
        Region region2 = GetRegionByName(regionName);
        City city2 = GetCity(regionName, subregionName, cityName);

        if (region2.MinLat >= city2.Lat || city2.Lat >= region2.MaxLat)
            throw new ExtendedException($"City '{cityName}' has latitude outside region '{regionName}'");

        if (region2.MinLon >= city2.Lon || city2.Lon >= region2.MaxLon)
            throw new ExtendedException($"City '{cityName}' has longitude outside region '{regionName}'");

        int num = 1 + (int)((region2.MaxLon - region2.MinLon) / region2.GridResolution);
        int num2 = 1 + (int)((region2.MaxLat - region2.MinLat) / region2.GridResolution);
        int c = (int)(num * (city2.Lon - region2.MinLon) / (region2.MaxLon - region2.MinLon));
        int r = (int)(num2 * (region2.MaxLat - city2.Lat) / (region2.MaxLat - region2.MinLat));

        return new TCS.GridCoordinates
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



    public CityDetail SaveCity(CityDetail city, out bool inserted)
    {
        City dbu = null;
        inserted = false;

        try
        {
            var id = city.Id;

            dbu = _dbContext.Cities.FirstOrDefault(a => id == a.Id);

            bool newEntry = dbu == null;

            dbu ??= new City();

            dbu.Subregion = city.Subregion;
            dbu.Name = city.Name;
            dbu.Lat = city.Lat;
            dbu.Lon = city.Lon;

            if (city.RegionId > 0)
                dbu.RegionId = city.RegionId;
            else
                dbu.RegionId = GetRegionByName(city.RegionName)?.Id ?? FirstRegion.Id;

            if (newEntry)
            {
                dbu.IsDefault = false;

                if (_dbContext.Insert(dbu) > 0)
                    inserted = true;
                else
                    dbu = null;
            }
            else
            {
                _dbContext.BeginTransaction();

                try
                {
                    if (city.IsDefault && !dbu.IsDefault)
                    {
                        // we want to mark this city as default for the supplied region/subregion
                        // the first step is to reset the default flag for all cities in the supplied region/subregion
                        var query = $"UPDATE City SET IsDefault=0 WHERE RegionId={dbu.RegionId} AND Subregion='{dbu.Subregion}'";
                        _dbContext.ExecuteSqlRaw(query);

                        // the second step is save the city as default. which means we need to set it as such here.
                        dbu.IsDefault = true;
                    }

                    if (_dbContext.Update(dbu) <= 0)
                        dbu = null;

                    _dbContext.CommitTransaction();
                }
                catch
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }
        catch (ExtendedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogException(ex);
            dbu = null;
        }

        return GetCityDetail(dbu);
    }

    public int DeleteCity(int cityId)
    {

        try
        {
            var dbu = _dbContext.Cities.FirstOrDefault(c => c.Id == cityId);
            if (dbu == null)
                return StatusCodes.Status404NotFound;

            if (dbu.IsDefault)
                throw new ExtendedException(GeographyServiceErrors.CannotDeleteDefaultCity);

            if (_dbContext.Delete(dbu) > 0)
                return StatusCodes.Status200OK;
        }
        catch (ExtendedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return StatusCodes.Status400BadRequest;
    }

    #endregion

    private CityDetail GetCityDetail(City c)
    {
        var region = _dbContext.Regions.FirstOrDefault(r => r.Id == c.RegionId);
        return new CityDetail
        {
            IsDefault = c.IsDefault,
            Id = c.Id,
            Lat = c.Lat,
            Lon = c.Lon,
            Name = c.Name,
            RegionName = region?.Name,
            RegionCode = region.Code,
            Subregion = c.Subregion,
            RegionId = c.RegionId,
        };
    }

    Task<GeoLocation> IGeographyService.GetGeoLocation(string ipAddress)
        => _geoLocationGateway.GetGeoLocation(ipAddress);
}
