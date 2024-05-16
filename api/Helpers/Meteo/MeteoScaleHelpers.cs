using ocpa.ro.api.Helpers.Generic;
using ThorusCommon.IO;

namespace ocpa.ro.api.Helpers.Meteo
{
    public class MeteoScaleHelper
    {
        protected IniFileHelper IniFile { get; private set; }

        public MeteoScaleHelper(IniFileHelper iniFile)
        {
            IniFile = iniFile;
        }
    }

    public class TemperatureScaleHelper : MeteoScaleHelper
    {
        public TemperatureScaleHelper(IniFileHelper iniFile) : base(iniFile)
        {
        }

        public float Colder => IniFile.ReadIniValue("Temperature", "Colder", -10f);
        public float Cold => IniFile.ReadIniValue("Temperature", "Cold", -5f);
        public float Warm => IniFile.ReadIniValue("Temperature", "Warm", 5f);
        public float Warmer => IniFile.ReadIniValue("Temperature", "Warmer", 10f);
        public float Hot => IniFile.ReadIniValue("Temperature", "Hot", 35f);
        public float Frost => IniFile.ReadIniValue("Temperature", "Frost", -10f);
    }

    public class PrecipScaleHelper : MeteoScaleHelper
    {
        public PrecipScaleHelper(IniFileHelper iniFile) : base(iniFile)
        {
        }

        public float Weak => IniFile.ReadIniValue("Precip", "Weak", 5f);
        public float Moderate => IniFile.ReadIniValue("Precip", "Moderate", 15f);
        public float Heavy => IniFile.ReadIniValue("Precip", "Heavy", 30f);
        public float Extreme => IniFile.ReadIniValue("Precip", "Extreme", 60f);
    }

    public class InstabilityScaleHelper : MeteoScaleHelper
    {
        public InstabilityScaleHelper(IniFileHelper iniFile) : base(iniFile)
        {
        }

        public float Weak => IniFile.ReadIniValue("Instability", "Weak", -6f);
        public float Moderate => IniFile.ReadIniValue("Instability", "Moderate", -2f);
        public float Heavy => IniFile.ReadIniValue("Instability", "Heavy", 2f);
        public float Extreme => IniFile.ReadIniValue("Instability", "Extreme", 6f);
    }

    public class FogScaleHelper : MeteoScaleHelper
    {
        public FogScaleHelper(IniFileHelper iniFile) : base(iniFile)
        {
        }

        public float Weak => IniFile.ReadIniValue("Fog", "Weak", 40f);
        public float Moderate => IniFile.ReadIniValue("Fog", "Moderate", 30f);
        public float Heavy => IniFile.ReadIniValue("Fog", "Heavy", 20f);
        public float Extreme => IniFile.ReadIniValue("Fog", "Extreme", 10f);
    }

    public class WindScaleHelper : MeteoScaleHelper
    {
        public WindScaleHelper(IniFileHelper iniFile) : base(iniFile)
        {
        }

        public float Weak => IniFile.ReadIniValue("Wind", "Weak", 10f);
        public float Moderate => IniFile.ReadIniValue("Wind", "Moderate", 30f);
        public float Heavy => IniFile.ReadIniValue("Wind", "Heavy", 50f);
        public float Extreme => IniFile.ReadIniValue("Wind", "Extreme", 70f);
    }

    public class BoundariesHelper : MeteoScaleHelper, IPrecipTypeBoundaries
    {
        public BoundariesHelper(IniFileHelper iniFile) : base(iniFile)
        {
        }

        public float MaxTeForSolidPrecip => IniFile.ReadIniValue("Boundaries", "MaxTeForSolidPrecip", 5f);
        public float MinTeForLiquidPrecip => IniFile.ReadIniValue("Boundaries", "MinTeForLiquidPrecip", 5f);
        public float MinTsForMelting => IniFile.ReadIniValue("Boundaries", "MinTsForMelting", 5f);
        public float MaxTsForFreezing => IniFile.ReadIniValue("Boundaries", "MaxTsForFreezing", 5f);
        public float MaxFreezingRainDelta => IniFile.ReadIniValue("Boundaries", "MaxFreezingRainDelta", 5f);
    }


    public class MeteoScaleHelpers
    {
        public TemperatureScaleHelper Temperature { get; private set; }

        public PrecipScaleHelper Precip { get; private set; }

        public InstabilityScaleHelper Instability { get; private set; }

        public BoundariesHelper Boundaries { get; private set; }

        public FogScaleHelper Fog { get; private set; }

        public WindScaleHelper Wind { get; private set; }

        public MeteoScaleHelpers(IniFileHelper iniFile)
        {
            Temperature = new TemperatureScaleHelper(iniFile);
            Precip = new PrecipScaleHelper(iniFile);
            Instability = new InstabilityScaleHelper(iniFile);
            Boundaries = new BoundariesHelper(iniFile);
            Fog = new FogScaleHelper(iniFile);
            Wind = new WindScaleHelper(iniFile);
        }
    }

}
