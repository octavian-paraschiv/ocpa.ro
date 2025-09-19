using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Models.Meteo;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCS = ThorusCommon.SQLite;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IGeographyService
{
    string FirstRegionCode { get; }
    IEnumerable<string> GetRegionCodes();
    IEnumerable<string> GetRegionNames();
    IEnumerable<string> GetSubregionNames(string regionName);
    IEnumerable<string> GetCityNames(string regionName, string subregionName);
    IEnumerable<CityDetail> GetAllCities();
    Region GetRegionByName(string regionName);
    Region GetRegionByCode(string regionCode);
    void ValidateSubregion(string regionName, string subregionName);
    CityDetail GetCity(string regionName, string subregionName, string cityName);
    TCS.GridCoordinates GetGridCoordinates(string regionName, string subregionName, string cityName);
    Task<GeoLocation> GetGeoLocation(string ipAddress);
    CityDetail SaveCity(CityDetail city, out bool inserted);
    int DeleteCity(int cityId);
    IEnumerable<RegionDetail> GetAllRegions();
}