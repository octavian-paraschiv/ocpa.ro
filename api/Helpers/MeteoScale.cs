using ThorusCommon.IO;

namespace api.Helpers
{
	public class MeteoScale
	{
		protected IniFile _iniFile;

		public MeteoScale(IniFile iniFile)
		{
			_iniFile = iniFile;
		}
	}

	public class Temperature : MeteoScale
	{
		public Temperature(IniFile iniFile) : base(iniFile)
        {
        }

		public float Colder => _iniFile.ReadIniValue("Temperature", "Colder", -10f);
		public float Cold => _iniFile.ReadIniValue("Temperature", "Cold", -5f);
		public float Warm => _iniFile.ReadIniValue("Temperature", "Warm", 5f);
		public float Warmer => _iniFile.ReadIniValue("Temperature", "Warmer", 10f);
		public float Hot => _iniFile.ReadIniValue("Temperature", "Hot", 35f);
		public float Frost => _iniFile.ReadIniValue("Temperature", "Frost", -10f);
	}

	public class Precip : MeteoScale
	{
		public Precip(IniFile iniFile) : base(iniFile)
		{
		}
	
		public float Weak => _iniFile.ReadIniValue("Precip", "Weak", 5f);
		public float Moderate => _iniFile.ReadIniValue("Precip", "Moderate", 15f);
		public float Heavy => _iniFile.ReadIniValue("Precip", "Heavy", 30f);
		public float Extreme => _iniFile.ReadIniValue("Precip", "Extreme", 60f);
	}

	public class Instability : MeteoScale
	{
		public Instability(IniFile iniFile) : base(iniFile)
		{
		}

		public float Weak => _iniFile.ReadIniValue("Instability", "Weak", -6f);
		public float Moderate => _iniFile.ReadIniValue("Instability", "Moderate", -2f);
		public float Heavy => _iniFile.ReadIniValue("Instability", "Heavy", 2f);
		public float Extreme => _iniFile.ReadIniValue("Instability", "Extreme", 6f);
	}

	public class Fog : MeteoScale
	{
		public Fog(IniFile iniFile) : base(iniFile)
		{
		}

		public float Weak => _iniFile.ReadIniValue("Fog", "Weak", 40f);
		public float Moderate => _iniFile.ReadIniValue("Fog", "Moderate", 30f);
		public float Heavy => _iniFile.ReadIniValue("Fog", "Heavy", 20f);
		public float Extreme => _iniFile.ReadIniValue("Fog", "Extreme", 10f);
	}

	public class Wind : MeteoScale
	{
		public Wind(IniFile iniFile) : base(iniFile)
		{
		}

		public float Weak => _iniFile.ReadIniValue("Wind", "Weak", 10f);
		public float Moderate => _iniFile.ReadIniValue("Wind", "Moderate", 30f);
		public float Heavy => _iniFile.ReadIniValue("Wind", "Heavy", 50f);
		public float Extreme => _iniFile.ReadIniValue("Wind", "Extreme", 70f);
	}

	public class Boundaries : MeteoScale, IPrecipTypeBoundaries
	{
		public Boundaries(IniFile iniFile) : base(iniFile)
		{
		}

		public float MaxTeForSolidPrecip => _iniFile.ReadIniValue("Boundaries", "MaxTeForSolidPrecip", 5f);
		public float MinTeForLiquidPrecip => _iniFile.ReadIniValue("Boundaries", "MinTeForLiquidPrecip", 5f);
		public float MinTsForMelting => _iniFile.ReadIniValue("Boundaries", "MinTsForMelting", 5f);
		public float MaxTsForFreezing => _iniFile.ReadIniValue("Boundaries", "MaxTsForFreezing", 5f);
		public float MaxFreezingRainDelta => _iniFile.ReadIniValue("Boundaries", "MaxFreezingRainDelta", 5f);
	}


	public class MeteoScaleSettings
	{
		public Temperature Temperature { get; private set; }

		public Precip Precip { get; private set; }

		public Instability Instability { get; private set; }

		public Boundaries Boundaries { get; private set; }

		public Fog Fog { get; private set; }

		public Wind Wind { get; private set; }

		public MeteoScaleSettings(IniFile iniFile)
        {
			Temperature = new Temperature(iniFile);
			Precip = new Precip(iniFile);
			Instability = new Instability(iniFile);
			Boundaries = new Boundaries(iniFile);
			Fog = new Fog(iniFile);
			Wind = new Wind(iniFile);
		}
	}

}
