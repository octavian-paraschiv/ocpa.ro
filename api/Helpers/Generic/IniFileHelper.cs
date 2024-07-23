using IniParser;
using IniParser.Model;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.Globalization;
using System.IO;

namespace ocpa.ro.api.Helpers.Generic
{
    public class IniFileHelper : BaseHelper
    {
        private readonly string _iniFilePath;
        private IniData _iniData;

        public IniFileHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, string iniFilePath)
            : base(hostingEnvironment, logger)
        {
            _iniFilePath = iniFilePath;

            ReadIniFile();

            var watchConfigFile = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_iniFilePath),
                Filter = Path.GetFileName(_iniFilePath),
                EnableRaisingEvents = true
            };

            watchConfigFile.Changed += (s, e) =>
            {
                try
                {
                    WatcherChangeTypes changeType = e.ChangeType;
                    WatcherChangeTypes watcherChangeTypes = changeType;
                    if (watcherChangeTypes == WatcherChangeTypes.Created || watcherChangeTypes == WatcherChangeTypes.Changed)
                    {
                        ReadIniFile();
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            };
        }

        private void ReadIniFile()
        {
            _iniData = new FileIniDataParser().ReadFile(_iniFilePath);
        }

        public T ReadIniValue<T>(string section, string key, T defValue = default)
        {
            T result;
            try
            {
                string text = _iniData[section][key];
                if (string.IsNullOrEmpty(text))
                {
                    result = defValue;
                }
                else if (typeof(T) == typeof(string))
                {
                    result = defValue;
                }
                else
                {
                    try
                    {
                        if (typeof(T).IsSubclassOf(typeof(Enum)))
                        {
                            result = (T)Enum.Parse(typeof(T), text);
                        }
                        else if (typeof(T) == typeof(TimeSpan))
                        {
                            TimeSpan timeSpan = TimeSpan.Parse(text);
                            result = (T)Convert.ChangeType(timeSpan, typeof(T), CultureInfo.InvariantCulture);
                        }
                        else if (typeof(T) == typeof(DateTime))
                        {
                            DateTime dateTime = DateTime.Parse(text);
                            result = (T)Convert.ChangeType(dateTime, typeof(T), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            try
                            {
                                Type typeFromHandle = typeof(T);
                                result = (T)Convert.ChangeType(text, typeFromHandle, CultureInfo.InvariantCulture);
                            }
                            catch (InvalidCastException)
                            {
                                result = (T)Enum.Parse(typeof(T), text);
                            }
                        }
                    }
                    catch
                    {
                        result = defValue;
                    }
                }
            }
            catch
            {
                result = defValue;
            }

            return result;
        }
    }

}
