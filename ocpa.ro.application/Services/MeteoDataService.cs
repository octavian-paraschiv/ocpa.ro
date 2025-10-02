using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Extensions;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThorusCommon.SQLite;

namespace ocpa.ro.application.Services
{


    public class MeteoDataService : BaseService, IMeteoDataService
    {
        #region Constants
        public const int DbCount = 10;
        #endregion

        #region Private members

        private readonly string _dataFolder;
        private readonly string[] _dbPaths = new string[DbCount];
        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly IGeographyService _geographyService;
        private readonly IWeatherTypeService _weatherTypeService;

        #endregion

        #region Constructor (DI)

        public MeteoDataService(IHostingEnvironmentService hostingEnvironment, ILogger logger,
            IGeographyService geographyHelper, IWeatherTypeService weatherTypeService)
            : base(hostingEnvironment, logger)
        {
            _geographyService = geographyHelper ?? throw new ArgumentNullException(nameof(geographyHelper));
            _weatherTypeService = weatherTypeService ?? throw new ArgumentNullException(nameof(weatherTypeService));

            _dataFolder = Path.Combine(hostingEnvironment.ContentPath, "Meteo");

            for (int i = 0; i < _dbPaths.Length; i++)
            {
                var dbName = i == 0 ? "Snapshot.db3" : $"Preview{i - 1}.db3";
                _dbPaths[i] = Path.Combine(_dataFolder, dbName);

                // "Touch" the databases to ensure they're created
                using var db = MeteoDB.OpenOrCreate(_dbPaths[i], false);
                _ = db.GetData(take: 1);
            }
        }

        #endregion

        #region IMeteoDataHelper implementation
        public async Task DropPreviewDatabase(int dbi)
        {
            int idx = DbiToIdx(dbi, false);

            try
            {
                await _lock.WaitAsync();
                if (File.Exists(_dbPaths[idx]))
                    File.Delete(_dbPaths[idx]);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SavePreviewDatabase(int dbi, byte[] data)
        {
            int idx = DbiToIdx(dbi, false);

            using MemoryStream input = new(data);
            using GZipStream zipped = new(input, CompressionMode.Decompress);
            using MemoryStream unzipped = new();

            await zipped.CopyToAsync(unzipped);

            try
            {
                await _lock.WaitAsync();
                await File.WriteAllBytesAsync(_dbPaths[idx], unzipped.ToArray());
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task PromotePreviewDatabase(int dbi)
        {
            int idx = DbiToIdx(dbi, false);

            try
            {
                await _lock.WaitAsync();
                File.Copy(_dbPaths[idx], _dbPaths[0], true);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<MeteoData> GetMeteoData(int dbi, GridCoordinates gc, string region, int skip, int take)
        {
            int idx = DbiToIdx(dbi, true);

            try
            {
                await _lock.WaitAsync();
                using var db = MeteoDB.OpenOrCreate(_dbPaths[idx], false);
                var data = GetMeteoData(db, gc, region, skip, take);
                data.Name = Path.GetFileName(_dbPaths[idx]);
                data.Dbi = dbi;
                return data;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IEnumerable<MeteoDbInfo>> GetDatabases()
        {
            List<MeteoDbInfo> dbInfos = [];

            try
            {
                await _lock.WaitAsync();
                for (int i = 0; i < _dbPaths.Length; i++)
                {
                    using var db = MeteoDB.OpenOrCreate(_dbPaths[i], false);
                    var range = GetCalendarRange(db, 0);
                    var name = Path.GetFileName(_dbPaths[i]);
                    var online = string.Equals(name, "Snapshot.db3", System.StringComparison.OrdinalIgnoreCase);
                    var modifyable = !online && !string.Equals(name, "Preview3.db3", System.StringComparison.OrdinalIgnoreCase);

                    MeteoDbInfo info = new()
                    {
                        CalendarRange = range,
                        Dbi = i - 1,
                        Name = name,
                        Status = online ?
                            MeteoDbStatus.Online : range.Length > 0 ?
                                MeteoDbStatus.Offline : MeteoDbStatus.Absent,
                    };

                    dbInfos.Add(info);
                }
            }
            finally
            {
                _lock.Release();
            }

            return dbInfos;
        }

        #endregion

        #region Private methods

        private static int DbiToIdx(int dbi, bool includeOnlineDb)
        {
            int idx = dbi + 1;
            int lowLimit = includeOnlineDb ? 0 : 1;
            int highLimit = DbCount - 1;

            if (lowLimit <= idx && idx <= highLimit)
                return idx;

            throw new ArgumentOutOfRangeException(nameof(dbi), $"Must be in range [{lowLimit - 1}..{highLimit - 1}]");
        }

        private MeteoData GetMeteoData(MeteoDB database, GridCoordinates gc, string region, int skip, int take)
        {
            MeteoData meteoData = new() { GridCoordinates = gc };

            try
            {
                CalendarRange range = GetCalendarRange(database, 0);

                meteoData.CalendarRange = new CalendarRange
                {
                    Start = range.Start.AddDays(skip)
                };

                skip = Math.Min(range.Length - 1, Math.Max(0, skip));

                if (take > 0)
                    take = Math.Min(range.Length - skip, take);
                else
                    take = range.Length - skip;

                var allData = GetData(database, region, gc, skip, take);
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

        private CalendarRange GetCalendarRange(MeteoDB database, int days)
        {
            CalendarRange result = new();

            try
            {
                var x = database
                    .GetData(_geographyService.FirstRegion.Code)
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
                        Start = DateTime.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        End = DateTime.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture),
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

        private IEnumerable<Data> GetData(MeteoDB database, string regionName, GridCoordinates gc, int skip, int take)
        {
            var rgn = _geographyService.GetRegionByName(regionName);
            return database.GetData(rgn.Code.ToUpper(), gc, skip, take);
        }

        #endregion
    }
}
