using IniParser;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ThorusCommon.IO;

namespace Meteo
{
    public static class ScaleSettings
    {
        public static readonly _Temperature Temperature = new _Temperature();

        public static readonly _Precip Precip = new _Precip();

        public static readonly _Instability Instability = new _Instability();

        public static readonly _Boundaries Boundaries = new _Boundaries();

        public static readonly _Fog Fog = new _Fog();

        public static readonly _Wind Wind = new _Wind();

        public static string AppRootPath { get; set; }

        public class _Temperature
        {
            public float Colder { get { return ReadIniValue("Temperature", "Colder", -10f); } }
            public float Cold { get { return ReadIniValue("Temperature", "Cold", -5f); } }
            public float Warm { get { return ReadIniValue("Temperature", "Warm", 5f); } }
            public float Warmer { get { return ReadIniValue("Temperature", "Warmer", 10f); } }
            public float Hot { get { return ReadIniValue("Temperature", "Hot", 35f); } }
            public float Frost { get { return ReadIniValue("Temperature", "Frost", -10f); } }
        }

        public class _Precip
        {
            public float Weak { get { return ReadIniValue("Precip", "Weak", 5f); } }
            public float Moderate { get { return ReadIniValue("Precip", "Moderate", 15f); } }
            public float Heavy { get { return ReadIniValue("Precip", "Heavy", 30f); } }
            public float Extreme { get { return ReadIniValue("Precip", "Extreme", 60f); } }
        }

        public class _Instability
        {
            public float Weak { get { return ReadIniValue("Instability", "Weak", -6f); } }
            public float Moderate { get { return ReadIniValue("Instability", "Moderate", -2f); } }
            public float Heavy { get { return ReadIniValue("Instability", "Heavy", 2f); } }
            public float Extreme { get { return ReadIniValue("Instability", "Extreme", 6f); } }
        }

        public class _Fog
        {
            public float Weak { get { return ReadIniValue("Fog", "Weak", 40f); } }
            public float Moderate { get { return ReadIniValue("Fog", "Moderate", 30f); } }
            public float Heavy { get { return ReadIniValue("Fog", "Heavy", 20f); } }
            public float Extreme { get { return ReadIniValue("Fog", "Extreme", 10f); } }
        }

        public class _Wind
        {
            public float Weak { get { return ReadIniValue("Wind", "Weak", 10f); } }
            public float Moderate { get { return ReadIniValue("Wind", "Moderate", 30f); } }
            public float Heavy { get { return ReadIniValue("Wind", "Heavy", 50f); } }
            public float Extreme { get { return ReadIniValue("Wind", "Extreme", 70f); } }
        }

        public class _Boundaries : IPrecipTypeBoundaries
        {
            public float MaxTeForSolidPrecip { get { return ReadIniValue("Boundaries", "MaxTeForSolidPrecip", 5f); } }
            public float MinTeForLiquidPrecip { get { return ReadIniValue("Boundaries", "MinTeForLiquidPrecip", 5f); } }
            public float MinTsForMelting { get { return ReadIniValue("Boundaries", "MinTsForMelting", 5f); } }
            public float MaxTsForFreezing { get { return ReadIniValue("Boundaries", "MaxTsForFreezing", 5f); } }
            public float MaxFreezingRainDelta { get { return ReadIniValue("Boundaries", "MaxFreezingRainDelta", 5f); } }
        }

        public static T ReadIniValue<T>(string section, string key, T defValue = default(T))
        {
            T retVal = defValue;

            try
            {
                string iniFilePath = Path.Combine(AppRootPath, "config\\ScaleSettings.ini");
                retVal = IniFile.Read(section, key, iniFilePath, defValue);
            }
            catch
            {
                retVal = defValue;
            }

            return retVal;
        }
    }

    public class IniFile
    {
        string _content = "";

        public IniFile()
        {
        }

        public static T Read<T>(string section, string key, string fileName, T defValue = default(T))
        {
            T retVal = defValue;

            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.ASCII))
                {
                    FileIniDataParser parser = new FileIniDataParser();

                    var iniData = parser.ReadData(sr);
                    string valueData = iniData[section][key];

                    if (string.IsNullOrEmpty(valueData))
                    {
                        retVal = defValue;
                    }
                    else
                    {
                        if (typeof(T) == typeof(String))
                        {
                            retVal = defValue;
                        }
                        else
                        {
                            try
                            {
                                if (typeof(T).IsSubclassOf(typeof(Enum)))
                                {
                                    retVal = (T)Enum.Parse(typeof(T), valueData);
                                }

                                else if (typeof(T) == typeof(TimeSpan))
                                {
                                    TimeSpan ts = TimeSpan.Parse(valueData);
                                    retVal = (T)Convert.ChangeType(ts, typeof(T), CultureInfo.InvariantCulture);
                                }

                                else if (typeof(T) == typeof(DateTime))
                                {
                                    DateTime dt = DateTime.Parse(valueData);
                                    retVal = (T)Convert.ChangeType(dt, typeof(T), CultureInfo.InvariantCulture);
                                }

                                else
                                {
                                    try
                                    {
                                        var t = typeof(T);
                                        retVal = (T)Convert.ChangeType(valueData, t, CultureInfo.InvariantCulture);
                                    }
                                    catch (InvalidCastException)
                                    {
                                        retVal = (T)Enum.Parse(typeof(T), valueData);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                retVal = defValue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retVal = defValue;
            }

            return retVal;
        }
    }
}