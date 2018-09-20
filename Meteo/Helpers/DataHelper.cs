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
    public static class DataHelper
    {
        static DataHelper()
        {
            try
            {
                string parentDataFolder = Path.Combine(AppFolders.DataFolder, "..");
                string dataArchiveFile = Path.Combine(parentDataFolder, "Data.zip");
                if (File.Exists(dataArchiveFile))
                {
                    if (ExtractZipFile(dataArchiveFile, "", parentDataFolder))
                        File.Delete(dataArchiveFile);
                }
            }
            catch { }
        }

        public static int GetCalendarRange(ref DateTime dtStart, ref DateTime dtEnd, int days)
        {
            string[] files = GetDataFiles("L_00_MAP*.thd", days);

            string fileTitle = Path.GetFileNameWithoutExtension(files[0]);
            string datePart = fileTitle.Substring(9, 10);

            dtStart = DateTime.ParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            fileTitle = Path.GetFileNameWithoutExtension(files[files.Length - 1]);
            datePart = fileTitle.Substring(9, 10);

            dtEnd = DateTime.ParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            return files.Length;
        }

        public static float GetDataPoint(string dataType, DateTime dt, int r, int c)
        {
            string dtStr = dt.ToString("yyyy-MM-dd");
            string fileName = $"{dataType}_MAP_{dtStr}_00.thd";
            string filePath = Path.Combine(AppFolders.DataFolder, fileName);

            //return new MatrixFile(filePath, true).Matrix[r, c];
            return DataReader.ReadFromFile(filePath, r, c);
        }

        public static string[] GetDataFiles(string mask, int count)
        {
            string[] files = Directory.GetFiles(AppFolders.DataFolder, mask);

            if (count < 1)
                return files;

            string[] ret = new string[count];

            Array.Copy(files, ret, count);

            return ret;

        }

        public static bool ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
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