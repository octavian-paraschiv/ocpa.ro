using common;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using ThorusCommon.IO;

namespace Meteo.Helpers
{
    public class MeteoDataHelper
    {
        string _dataFolder = ".";

        public MeteoDataHelper(string dataFolder)
        {
            try
            {
                _dataFolder = dataFolder;
                string parentDataFolder = Path.Combine(dataFolder, "..");
                string dataArchiveFile = Path.Combine(parentDataFolder, "Data.tha");
                if (File.Exists(dataArchiveFile))
                {
                    if (ExtractArchive(dataArchiveFile, "", parentDataFolder))
                        File.Delete(dataArchiveFile);
                }
            }
            catch { }
        }

        public List<string> GetDataTypes(DateTime dt)
        {
            List<string> types = new List<string>();

            try
            {
                string lookupMask = $"_MAP_{dt:yyyy-MM-dd}_00.thd";
                string[] files = GetDataFiles("*" + lookupMask, 0);
                types = (from f in files
                         select Path.GetFileName(f).Replace(lookupMask, "")).ToList();
            }
            catch
            {
            }

            return types;
        }

        public CalendarRange GetCalendarRange(int days)
        {
            var range = new CalendarRange();

            try
            {
                string[] files = GetDataFiles("L_00_MAP*.thd", days);

                string fileTitle = Path.GetFileNameWithoutExtension(files[0]);
                string datePart = fileTitle.Substring(9, 10);

                DateTime dtStart = DateTime.ParseExact(datePart, MeteoConstants.DateFormat, CultureInfo.InvariantCulture);

                fileTitle = Path.GetFileNameWithoutExtension(files[files.Length - 1]);
                datePart = fileTitle.Substring(9, 10);

                DateTime dtEnd = DateTime.ParseExact(datePart, MeteoConstants.DateFormat, CultureInfo.InvariantCulture);

                range = new CalendarRange
                {
                    Start = dtStart,
                    End = dtEnd,
                    Length = files.Length
                };
            }
            catch
            {
            }

            return range;
        }

        public float GetDataPoint(string dataType, DateTime dt, GridCoordinates gc)
        {
            string dtStr = dt.ToString(MeteoConstants.DateFormat);
            string fileName = $"{dataType}_MAP_{dtStr}_00.thd";
            string filePath = Path.Combine(_dataFolder, fileName);
            return DataReader.ReadFromFile(filePath, gc.R, gc.C);
        }

        public string[] GetDataFiles(string mask, int count)
        {
            string[] files = Directory.GetFiles(_dataFolder, mask);

            if (count < 1)
                return files;

            count = Math.Min(files.Length, count);

            string[] ret = new string[count];

            Array.Copy(files, ret, count);

            return ret;

        }

        public static bool ExtractArchive(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
    }
}