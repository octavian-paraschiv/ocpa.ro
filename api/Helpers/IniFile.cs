using IniParser;
using IniParser.Model;
using System;
using System.Globalization;
using System.IO;

namespace api.Helpers
{
    public class IniFile
    {
        private readonly string _iniFilePath;
        private readonly FileSystemWatcher _watchConfigFile;

        private IniData _iniData;

        public IniFile(string iniFilePath)
        {
            _iniFilePath = iniFilePath;

            ReadIniFile();

            _watchConfigFile = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_iniFilePath),
                Filter = Path.GetFileName(_iniFilePath),
                EnableRaisingEvents = true
            };
            _watchConfigFile.Changed += delegate (object s, FileSystemEventArgs e)
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
                catch
                {
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
                    catch (Exception)
                    {
                        result = defValue;
                    }
                }
            }
            catch (Exception)
            {
                result = defValue;
            }
            return result;
        }
    }

}
