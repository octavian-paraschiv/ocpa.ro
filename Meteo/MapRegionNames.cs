using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meteo
{
    public static class MapRegions
    {
        private static readonly string[] _names = new string[]
        {
            "Hungary",
            "Hungary",
            "Ucraine",
            "Ucraine",
            "Ucraine",
            "Ucraine",
            "Ucraine / Botosani North",
            "Ucraine",
            "Ucraine",
            "Ucraine",
            "Hungary",
            "Hungary",
            "Bihor / Satu-Mare",
            "Salaj / Maramures Vest",
            "Maramures Est / Bistrita-Nasaud",
            "Suceava Vest",
            "Suceava Est / Botosani Sud / Iasi Vest / Neamt Nord",
            "Iasi Est / Moldova",
            "Moldova",
            "Moldova",
            "Hungary",
            "Arad Vest",
            "Arad Est / Bihor Sud / Hunedoara Nord",
            "Cluj / Alba Nord",
            "Mures / Sibiu Nord",
            "Harghita",
            "Bacau Est / Neamt Sud",
            "Bacau Vest / Vaslui",
            "Moldova",
            "Moldova",
            "Serbia",
            "Timis / Caras-Severin Vest",
            "Caras-Severin Est / Hunedoara Sud-Vest",
            "Hunedoara Sud-Est / Alba Sud/  Gorj Nord",
            "Sibiu Sud / Valcea Nord / Arges Nord /Brasov Vest",
            "Brasov Est / Covasna Vest / Dambovita Nord / Prahova Nord-Vest",
            "Covasna Est / Vrancea Vest / Buzau / Prahova Nord-Est",
            "Vrancea Est / Galati / Braila Nord",
            "Moldova / Tulcea Nord-Vest",
            "Ucraina / Tulcea Delta",
            "Serbia",
            "Serbia / Caras-Severin Sud",
            "Serbia / Mehedinti Vest",
            "Mehedinti Est / Gorj Sud / Dolj Nord",
            "Valcea Sud / Arges Sud-vest / Olt Nord",
            "Arges Sud-Est / Dambovita Sud / Teleorman Nord / Giurgiu Nord",
            "Bucuresti / Ilfov / Prahova Sud / Ialomita Vest / Calarasi Vest / Giurgiu Sud-Est",
            "Braila Sud / Ialomita Est / Calarasi Est / Constanta Sud-Vest",
            "Constanta",
            "Black Sea / Litoral Constanta",
            "Serbia",
            "Serbia",
            "Serbia/Bulgaria",
            "Bulgaria / Dolj Sud",
            "Bulgaria / Olt Sud",
            "Bulgaria / Teleorman Sud",
            "Bulgaria",
            "Bulgaria",
            "Bulgaria / Constanta Sud",
            "Black Sea",
        };

        public static string GetRegionName(int r, int c)
        {
            try
            {
                int idx = c + 10 * r;
                return _names[idx];
            }
            catch { }

            return "Unknown region";
        }
    }
}