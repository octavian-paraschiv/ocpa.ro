using api.Controllers.Models;
using api.Helpers;
using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Helpers.Meteo.Helpers;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Helpers
{
    public interface IMeteoDataHelper
    {
        void HandleDatabasePart(UploadDataPart part);
        void ReplaceDatabase(string base64);
        CalendarRange GetCalendarRange(int days);
        List<Data> GetData(string region, GridCoordinates gc, int skip, int take);
        MeteoScaleSettings Scale { get; }

    }

    public class MeteoDataHelper : IMeteoDataHelper
    {
        private WeatherTypeHelper _precipHelper;

        static MeteoDB _db = null;

        private MeteoScaleSettings _scale;
        private IniFile _iniFile;
        private string _dataFolder;

        public MeteoScaleSettings Scale => _scale;

        public MeteoDataHelper(IWebHostEnvironment hostingEnvironment)
        {
            string rootPath = Path.GetDirectoryName(hostingEnvironment.ContentRootPath);
            _dataFolder = Path.Combine(rootPath, "Content/Meteo");

            if (_db == null)
                _db = MeteoDB.OpenOrCreate(Path.Combine(_dataFolder, "Snapshot.db3"), false);

            _precipHelper = new WeatherTypeHelper(this);

            var iniPath = System.IO.Path.Combine(_dataFolder, "ScaleSettings.ini");
            _iniFile = new IniFile(iniPath);
            _scale = new MeteoScaleSettings(_iniFile);
        }

        public void HandleDatabasePart(UploadDataPart part)
        {
            List<string> partFiles = new List<string>();
            if (part.PartIndex == 0)
            {
                partFiles = Directory.GetFiles(_dataFolder, "db_*.part").ToList();
                if (partFiles?.Count > 0)
                    partFiles.ForEach(pf => File.Delete(pf));
            }

            File.WriteAllText(Path.Combine(_dataFolder, $"db_{part.PartIndex:d3}.part"), part.PartBase64);

            partFiles = Directory.GetFiles(_dataFolder, "db_*.part").OrderBy(pf => pf).ToList();
            if (partFiles?.Count == part.TotalParts)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string pf in partFiles)
                {
                    sb.Append(File.ReadAllText(pf));
                    File.Delete(pf);
                }

                ReplaceDatabase(sb.ToString());
            }
        }

        public void ReplaceDatabase(string base64)
        {
            byte[] data = Convert.FromBase64String(base64);

            using (MemoryStream input = new MemoryStream(data))
            using (GZipStream zipped = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream unzipped = new MemoryStream())
            {
                zipped.CopyTo(unzipped);
                File.WriteAllBytes(Path.Combine(_dataFolder, "Snapshot.db3"), unzipped.ToArray());
            }

            if (_db != null)
                _db.Close();

            _db = MeteoDB.OpenOrCreate(Path.Combine(_dataFolder, "Snapshot.db3"), false);
        }

        public CalendarRange GetCalendarRange(int days)
        {
            CalendarRange result = new CalendarRange();
            try
            {
                var x = _db.Data
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

        public List<Data> GetData(string region, GridCoordinates gc, int skip, int take)
        {
            var regionId = (from r in _db.Regions
                            where r.Name == region
                            select r.Id).FirstOrDefault();

            var x = _db.Data
                .Where(d => d.RegionId == regionId && d.R == gc.R && d.C == gc.C)
                .OrderBy(d => d.Timestamp)
                .Skip(skip)
                .Take(take);

            return x.ToList();
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
            T val = defaultValue;
            Type type = typeof(T);

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
