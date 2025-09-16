using ocpa.ro.domain.Models.Meteo;
using System.Collections.Generic;
using ThorusCommon.SQLite;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IWeatherTypeService
{
    string GetWeatherType(Data meteoData, int windSpeed, List<string> risks);
    (int windSpeed, WindDirection direction) GetWind(Data meteoData);
    string GetPrecipType(Data meteoData);
    string GetTempFeel(Data meteoData);
}