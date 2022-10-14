using api.Controllers.Models;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ThorusCommon.IO;

namespace ocpa.ro.api.Helpers
{
	public class MeteoDataHelper
	{
		private string _dataFolder = ".";

		public MeteoDataHelper(string dataFolder)
		{
			_dataFolder = dataFolder;
		}

		public List<string> GetDataTypes(DateTime dt)
		{
			List<string> result = new List<string>();
			try
			{
				string lookupMask = $"_MAP_{dt:yyyy-MM-dd}_00.thd";
				string[] dataFiles = GetDataFiles("*" + lookupMask, 0);
				result = dataFiles.Select((string f) => Path.GetFileName(f).Replace(lookupMask, "")).ToList();
			}
			catch
			{
			}
			return result;
		}

		public CalendarRange GetCalendarRange(int days)
		{
			CalendarRange result = new CalendarRange();
			try
			{
				string[] dataFiles = GetDataFiles("L_00_MAP*.thd", days);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dataFiles[0]);
				string s = fileNameWithoutExtension.Substring(9, 10);
				DateTime start = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);
				fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dataFiles[dataFiles.Length - 1]);
				s = fileNameWithoutExtension.Substring(9, 10);
				DateTime end = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);
				result = new CalendarRange
				{
					Start = start,
					End = end,
					Length = dataFiles.Length
				};
			}
			catch
			{
			}
			return result;
		}

		public float GetDataPoint(string dataType, DateTime dt, GridCoordinates gc)
		{
			string text = dt.ToString("yyyy-MM-dd");
			string path = dataType + "_MAP_" + text + "_00.thd";
			string filePath = Path.Combine(_dataFolder, path);
			return DataReader.ReadFromFile(filePath, gc.R, gc.C);
		}

		public string[] GetDataFiles(string mask, int count)
		{
			string[] files = Directory.GetFiles(_dataFolder, mask);
			if (count < 1)
			{
				return files;
			}
			count = Math.Min(files.Length, count);
			string[] array = new string[count];
			Array.Copy(files, array, count);
			return array;
		}
	}
}
