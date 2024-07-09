using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Models.Meteo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Helpers.Meteo
{
    public interface IMeteoDataHelper
    {
        Task ReplaceDatabase(int dbIdx, byte[] data);
        CalendarRange GetCalendarRange(int dbIdx, int days);
        MeteoData GetMeteoData(int dbIdx, GridCoordinates gc, string region, int skip, int take);
        MeteoScaleHelpers Scale { get; }
        public string LatestStudioFile { get; }
    }

    public class MeteoDataHelper : IMeteoDataHelper, IDisposable
    {
        public const int DbCount = 5;

        private readonly MeteoScaleHelpers _scale;
        private readonly IniFileHelper _iniFile;
        private readonly string _dataFolder;

        private MeteoDB[] _databases = new MeteoDB[DbCount];
        private readonly string[] _dbPaths = new string[DbCount];

        public MeteoScaleHelpers Scale => _scale;

        public MeteoDataHelper(IWebHostEnvironment hostingEnvironment)
        {
            _dataFolder = Path.Combine(hostingEnvironment.ContentPath(), "Meteo");

            for (int i = 0; i < _databases.Length; i++)
            {
                var dbName = i > 0 ? $"Preview{i}" : "Snapshot";
                _dbPaths[i] = Path.Combine(_dataFolder, $"{dbName}.db3");
                _databases[i] = MeteoDB.OpenOrCreate(_dbPaths[i], false);
            }

            var iniPath = Path.Combine(_dataFolder, "ScaleSettings.ini");
            _iniFile = new IniFileHelper(iniPath);
            _scale = new MeteoScaleHelpers(_iniFile);
        }

        public string LatestStudioFile
        {
            get
            {
                string path = Path.Combine(_dataFolder, $"current");
                if (path?.Length > 0 && Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "Thorus Weather Studio *.exe");
                    if (files?.Length > 0)
                    {
                        return files
                            .Select(f => Path.GetFileName(f))
                            .OrderByDescending(f => new Version(f.Replace("Thorus Weather Studio", "").Replace(".exe", "").Trim()))
                            .FirstOrDefault();
                    }
                }

                return null;
            }
        }

        public async Task ReplaceDatabase(int dbIdx, byte[] data)
        {
            _databases[dbIdx]?.Close();

            using (MemoryStream input = new MemoryStream(data))
            using (GZipStream zipped = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream unzipped = new MemoryStream())
            {
                await zipped.CopyToAsync(unzipped);

                var tmpFile = Path.GetTempFileName();
                await File.WriteAllBytesAsync(_dbPaths[dbIdx], unzipped.ToArray());
            }

            _databases[dbIdx] = MeteoDB.OpenOrCreate(_dbPaths[dbIdx], false);
        }

        public CalendarRange GetCalendarRange(int dbIdx, int days)
        {
            CalendarRange result = new CalendarRange();
            try
            {
                var x = _databases[dbIdx].Data
                    .Where(d => d.RegionId == 1 && d.R == 0 && d.C == 0)
                    .OrderBy(d => d.Timestamp)
                    .Distinct();

                var xx = x.Select(d => d.Timestamp).ToList();

                if (days == 0)
                    days = xx.Count;

                var start = xx[0];
                var end = xx[days - 1];

                result = new CalendarRange
                {
                    Start = DateTime.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    End = DateTime.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Length = days
                };
            }
            catch
            {
            }
            return result;
        }

        public MeteoData GetMeteoData(int dbIdx, GridCoordinates gc, string region, int skip, int take)
        {
            MeteoData meteoData = new MeteoData { GridCoordinates = gc };

            try
            {
                CalendarRange range = GetCalendarRange(dbIdx, 0);

                meteoData.CalendarRange = new CalendarRange
                {
                    Start = range.Start.AddDays(skip)
                };

                skip = Math.Min(range.Length - 1, Math.Max(0, skip));

                int remaining = range.Length - skip;

                take = Math.Min(range.Length - skip, Math.Max(0, range.Length));

                var allData = GetData(dbIdx, region, gc, skip, take);
                if (allData?.Count > 0)
                {
                    meteoData.Data = new Dictionary<string, MeteoDailyData>();
                    meteoData.CalendarRange.End = range.Start.AddDays(allData.Count - 1);
                    meteoData.CalendarRange.Length = allData.Count;

                    allData.ForEach(d =>
                    {
                        List<string> risks = new List<string>();
                        var wind = WeatherTypeHelper.GetWind(d, out string windDirection);
                        var forecast = WeatherTypeHelper.GetWeatherType(Scale, d, wind, risks);

                        MeteoDailyData value = new MeteoDailyData
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

                            TempFeel = WeatherTypeHelper.GetTempFeel(Scale, d),
                            Wind = wind,
                            WindDirection = windDirection,

                            Hazards = risks,
                            Forecast = forecast,
                        };

                        meteoData.Data.Add(d.Timestamp, value);
                    });
                }
            }
            catch
            {
            }
            return meteoData;
        }


        private List<Data> GetData(int dbIdx, string region, GridCoordinates gc, int skip, int take)
        {
            var regionId = (from r in _databases[dbIdx].Regions
                            where r.Name == region
                            select r.Id).FirstOrDefault();

            var x = _databases[dbIdx].Data
                .Where(d => d.RegionId == regionId && d.R == gc.R && d.C == gc.C)
                .OrderBy(d => d.Timestamp)
                .Skip(skip)
                .Take(take);

            return x.ToList();
        }

        public void Dispose()
        {
            foreach (var db in _databases)
                db?.SaveAndClose();
        }
    }

    public static class ExtensionMethods
    {
        public static int Round(this float input)
        {
            return (int)Math.Round(input);
        }

        public static T GetValue<T>(this Dictionary<string, float> data, string key, T defaultValue = default)
            where T : IComparable, IConvertible, IFormattable
        {
            Type type = typeof(T);

            T val;
            try
            {
                double raw = data[key];

                if (type != typeof(float) &&
                    type != typeof(double) &&
                    type != typeof(decimal))
                {
                    raw = Math.Round(raw, 0);
                }

                val = (T)Convert.ChangeType(raw, type);
            }
            catch
            {
                val = defaultValue;
            }

            return val;
        }
    }
}
