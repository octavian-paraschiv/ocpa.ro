using ocpa.ro.domain;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Entities.Meteo;
using ocpa.ro.domain.Exceptions;
using ocpa.ro.domain.Extensions;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ThorusCommon.SQLite;

namespace ocpa.ro.application.Services.Meteo;

public class MeteoDataService2 : BaseService, IMeteoDataService2
{
    private readonly IMeteoDbContext _dbContext;
    private readonly IGeographyService _geographyService;
    private readonly IWeatherTypeService _weatherTypeService;
    private readonly ISystemSettingsService _systemSettingsService;

    public MeteoDataService2(IHostingEnvironmentService hostingEnvironment,
        ILogger logger,
        IGeographyService geographyHelper,
        IWeatherTypeService weatherTypeService,
        ISystemSettingsService systemSettingsService,
        IMeteoDbContext dbContext)
        : base(hostingEnvironment, logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _geographyService = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
        _weatherTypeService = weatherTypeService ?? throw new ArgumentNullException(nameof(weatherTypeService));
        _systemSettingsService = systemSettingsService ?? throw new ArgumentNullException(nameof(systemSettingsService));
    }

    public IEnumerable<int> GetDbiList()
    {
        var activeDbi = _systemSettingsService.ActiveMeteoDbi;

        var dbis = _dbContext.Data
            .Select(d => d.Dbi)
            .Distinct()
            .Where(dbi => dbi != activeDbi)
            .AsEnumerable();

        return [activeDbi, .. dbis];
    }

    public IEnumerable<MeteoDbInfo> GetDatabases()
    {
        try
        {
            var activeDbi = _systemSettingsService.ActiveMeteoDbi;

            var data = _dbContext.Data.AsQueryable()
                .Select(d => d.Dbi)
                .Distinct()
                .Select(dbi => new MeteoDbInfo
                {
                    Dbi = dbi,
                    Name = $"Snapshot {dbi}",
                    Online = dbi == activeDbi,
                    Modifyable = dbi != activeDbi,

                }).ToList();

            data.ForEach(d => d.CalendarRange = GetCalendarRange((sbyte)d.Dbi, 0, null));

            return data;
        }
        catch (Exception ex)
        {
            LogException(ex);
            return null;
        }
    }

    public MeteoData GetMeteoData(int dbi, GridCoordinates gc, string region, int skip, int take)
    {
        try
        {
            var activeDbi = _systemSettingsService.ActiveMeteoDbi;

            if (dbi < 0)
                dbi = activeDbi;
            else
                ValidateDbi(dbi, false);

            var data = FetchMeteoData(dbi, gc, region, skip, take);

            data.Dbi = dbi;
            data.Name = $"Snapshot {dbi}";
            data.Online = dbi == activeDbi;
            data.Modifyable = dbi != activeDbi;

            return data;
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return null;
    }

    public void MakeActiveDbi(int dbi)
    {
        ValidateDbi(dbi, false);

        _systemSettingsService.ActiveMeteoDbi = dbi;
        var activeDbi = _systemSettingsService.ActiveMeteoDbi;
        if (dbi != activeDbi)
            throw new ExtendedException("ERR_MAKE_ACTIVE_FAILED");
    }

    public void SaveMeteoData(IEnumerable<MeteoDbData> data, bool purgeDbiRecords)
    {
        var first = data?.FirstOrDefault();
        if (first == null)
            throw new ExtendedException("ERR_NO_SAVE_ELEMENTS");

        ValidateDbi(first.Dbi, true);

        if (data.DistinctBy(d => d.Dbi).Count() > 1)
            throw new ExtendedException("ERR_NOT_SAME_DBI");

        if (data.DistinctBy(d => d.RegionCode).Count() > 1)
            throw new ExtendedException("ERR_NOT_SAME_REGION_CODE");

        if (data.DistinctBy(d => d.Timestamp).Count() > 1)
            throw new ExtendedException("ERR_NOT_SAME_TIMESTAMP");

        var rgn = _geographyService.GetRegionByCode(first.RegionCode); // Validate region code...

        if (first.R < 0 ||
            first.R > (int)((rgn.MaxLat - rgn.MinLat) / rgn.GridResolution))
            throw new ExtendedException("ERR_INVALID_GRID_ROW");

        if (first.C < 0 ||
            first.C > (int)((rgn.MaxLon - rgn.MinLon) / rgn.GridResolution))
            throw new ExtendedException("ERR_INVALID_GRID_COLUMN");

        if (purgeDbiRecords)
            _dbContext.BeginTransaction();

        try
        {
            if (purgeDbiRecords)
            {
                var query = $"DELETE FROM MeteoData WHERE Dbi={first.Dbi} and RegionCode='{first.RegionCode}'";
                _dbContext.ExecuteSqlRaw(query);
            }

            _dbContext.InsertRange(data);

            if (purgeDbiRecords)
                _dbContext.CommitTransaction();

            Console.WriteLine($"Inserted data for dbi={first.Dbi} region={first.RegionCode} Timestamp={first.Timestamp}");
        }
        catch (Exception ex)
        {
            if (purgeDbiRecords)
                _dbContext.RollbackTransaction();

            LogException(ex);
            throw;
        }
    }


    private CalendarRange GetCalendarRange(int dbi, int days, string region)
    {
        CalendarRange result = new();

        region ??= _geographyService.FirstRegion.Name;

        var rgn = _geographyService.GetRegionByName(region);

        try
        {
            var x = _dbContext.Data
                .Where(d => d.Dbi == dbi && d.RegionCode.ToUpper() == rgn.Code.ToUpper())
                .OrderBy(d => d.Timestamp)
                .Distinct();

            var xx = x.Select(d => d.Timestamp).ToList();

            if (days == 0)
                days = xx.Count;

            if (days > 0)
            {
                var start = xx[0];
                var end = xx[days - 1];

                result = new CalendarRange
                {
                    Start = xx[0],
                    End = xx[days - 1],
                    Length = days
                };
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return result;
    }

    private MeteoData FetchMeteoData(int dbi, GridCoordinates gc, string region, int skip, int take)
    {
        MeteoData meteoData = new() { GridCoordinates = gc, Region = region };

        try
        {
            CalendarRange range = GetCalendarRange(dbi, 0, region);

            meteoData.CalendarRange = new CalendarRange
            {
                Start = range.Start.AddDays(skip)
            };

            skip = Math.Min(range.Length - 1, Math.Max(0, skip));

            if (take > 0)
                take = Math.Min(range.Length - skip, take);
            else
                take = range.Length - skip;

            var allData = GetData(dbi, region, gc, skip, take);
            if (allData?.Any() ?? false)
            {
                meteoData.Data = [];
                meteoData.CalendarRange.End = range.Start.AddDays(allData.Count() - 1);
                meteoData.CalendarRange.Length = allData.Count();

                foreach (var d in allData)
                {
                    List<string> risks = [];
                    var (windSpeed, windDirection) = _weatherTypeService.GetWind(d);
                    var forecast = _weatherTypeService.GetWeatherType(d, windSpeed, risks);

                    MeteoDailyData value = new()
                    {
                        TMaxActual = d.T_SH.Round(),
                        TMinActual = d.T_SL.Round(),
                        TMaxNormal = d.T_NH.Round(),
                        TMinNormal = d.T_NL.Round(),
                        SnowCover = d.N_00.Round(),
                        Precip = d.C_00.Round(),
                        Instability = d.L_00.Round(),
                        Fog = -d.F_SI.Round() + 100,
                        SoilRain = d.R_00.Round(),
                        Rain = d.R_DD.Round(),
                        Snow = d.N_DD.Round(),
                        P00 = d.P_00.Round(),
                        P01 = d.P_01.Round(),

                        TempFeel = _weatherTypeService.GetTempFeel(d),
                        Wind = windSpeed,
                        WindDirection = windDirection,

                        Hazards = risks,
                        Forecast = forecast,
                    };

                    meteoData.Data.Add(d.Timestamp, value);
                }
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return meteoData;
    }

    private IEnumerable<Data> GetData(int dbi, string regionName, GridCoordinates gc, int skip, int take)
    {
        var rgn = _geographyService.GetRegionByName(regionName);

        return _dbContext.Data
            .Where(d => d.Dbi == dbi && d.RegionCode.ToUpper() == rgn.Code.ToUpper() && d.R == gc.R && d.C == gc.C)
            .Skip(skip)
            .Take(take)
            .Select(d => new Data
            {
                C = d.C,
                R = d.R,
                Id = d.Id,
                Timestamp = d.Timestamp.ToString(Constants.DateFormat, CultureInfo.InvariantCulture),
                RegionCode = d.RegionCode,

                C_00 = d.C_00 / 100f,
                F_SI = d.F_SI / 100f,
                L_00 = d.L_00 / 100f,
                N_00 = d.N_00 / 100f,
                N_DD = d.N_DD / 100f,
                P_00 = d.P_00 / 100f,
                P_01 = d.P_01 / 100f,
                R_00 = d.R_00 / 100f,
                R_DD = d.R_DD / 100f,
                T_01 = d.T_01 / 100f,
                T_NH = d.T_NH / 100f,
                T_NL = d.T_NL / 100f,
                T_SH = d.T_SH / 100f,
                T_SL = d.T_SL / 100f,
                T_TE = d.T_TE / 100f,
                T_TS = d.T_TS / 100f,
                W_00 = d.W_00 / 100f,
                W_01 = d.W_01 / 100f,
                W_10 = d.W_10 / 100f,
                W_11 = d.W_11 / 100f,

            })
            .AsEnumerable();
    }

    private void ValidateDbi(int dbi, bool forSave)
    {
        var dbis = _dbContext.Data.Select(d => d.Dbi).Distinct().AsEnumerable();

        if (forSave)
        {
            var activeDbi = _systemSettingsService.ActiveMeteoDbi;
            if (dbi == activeDbi)
                throw new ExtendedException("ERR_CANNOT_SAVE_ACTIVE");
        }
        else if (!dbis.Contains((sbyte)dbi))
            throw new ExtendedException("ERR_INVALID_DBI");
    }

}
