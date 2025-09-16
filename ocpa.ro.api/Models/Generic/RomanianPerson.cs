using ocpa.ro.api.Exceptions;
using ocpa.ro.api.Models.Medical;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ocpa.ro.api.Models.Generic
{
    public static class SirutaCodes
    {
        public static readonly ReadOnlyDictionary<string, string> Counties = new(new Dictionary<string, string>
        {
            { "01" , "Alba" },
            { "02" , "Arad" },
            { "03" , "Argeș" },
            { "04" , "Bacău" },
            { "05" , "Bihor" },
            { "06" , "Bistrița-Năsăud" },
            { "07" , "Botoșani" },
            { "08" , "Brașov" },
            { "09" , "Brăila" },
            { "10" , "Buzău" },
            { "11" , "Caraș-Severin" },
            { "12" , "Cluj" },
            { "13" , "Constanța" },
            { "14" , "Covasna" },
            { "15" , "Dâmbovița" },
            { "16" , "Dolj" },
            { "17" , "Galați" },
            { "18" , "Gorj" },
            { "19" , "Harghita" },
            { "20" , "Hunedoara" },
            { "21" , "Ialomița" },
            { "22" , "Iași" },
            { "23" , "Ilfov" },
            { "24" , "Maramureș" },
            { "25" , "Mehedinți" },
            { "26" , "Mureș" },
            { "27" , "Neamț" },
            { "28" , "Olt" },
            { "29" , "Prahova" },
            { "30" , "Satu Mare" },
            { "31" , "Sălaj" },
            { "32" , "Sibiu" },
            { "33" , "Suceava" },
            { "34" , "Teleorman" },
            { "35" , "Timiș" },
            { "36" , "Tulcea" },
            { "37" , "Vaslui" },
            { "38" , "Vâlcea" },
            { "39" , "Vrancea" },
            { "40" , "București" },
            { "41" , "București - Sector 1" },
            { "42" , "București - Sector 2" },
            { "43" , "București - Sector 3" },
            { "44" , "București - Sector 4" },
            { "45" , "București - Sector 5" },
            { "46" , "București - Sector 6" },
            { "51" , "Călărași" },
            { "52" , "Giurgiu" },
            { "47" , "Bucuresti - Sector 7 (desființat)" },
            { "48" , "Bucuresti - Sector 8 (desființat)" },
        });
    }

    public static class CnpValidator
    {
        // See https://ro.wikipedia.org/wiki/Cod_numeric_personal_(Rom%C3%A2nia)#Validare
        const string ControlConstant = "279146358279";
        public static string Validate(string cnp)
        {
            cnp = (cnp ?? "").Trim();
            if (cnp.Length != 13)
                throw new ExtendedException("ERROR_BAD_CNP");

            int sum = 0;
            for (int i = 0; i < ControlConstant.Length; i++)
            {
                int c1 = cnp[i] - '0';
                int c2 = ControlConstant[i] - '0';
                sum += c1 * c2;
            }

            int q = sum % 11;
            if (q == 10)
                q = 1;

            if (cnp.Last() != q + '0')
                throw new ExtendedException("ERROR_BAD_CNP");

            return cnp;
        }
    }

    public class RomanianPerson : Person
    {
        public string Gender { get; private set; }
        public string BirthDate { get; private set; }
        public string BirthPlace { get; private set; }
        public string NNN { get; private set; }

        public RomanianPerson(Person person)
        {
            Id = person.Id;
            Name = person.Name;
            Comment = person.Comment;

            try
            {
                CNP = CnpValidator.Validate(person.CNP);
            }
            catch
            {
                throw new ExtendedException("ERROR_BAD_CNP_IN_DATABASE");
            }

            ParseCnp();
        }

        private void ParseCnp()
        {
            char g = CNP.First();
            string date = CNP.Substring(1, 6);
            string birthPlace = CNP.Substring(7, 2);
            NNN = CNP.Substring(9, 3);

            Gender = g % 2 == 0 ? "F" : "M";

            string dateFmt = "yyyyMMdd";

            switch (g)
            {
                case '1':
                case '2':
                    date = "19" + date;
                    break;

                case '3':
                case '4':
                    date = "18" + date;
                    break;

                case '5':
                case '6':
                    date = "20" + date;
                    break;

                case '7':
                case '8':
                    // Unable to extract the century part, so assume 2-year digit
                    dateFmt = "yyMMdd";
                    break;
            }

            BirthDate = DateTime
                .ParseExact(date, dateFmt, CultureInfo.InvariantCulture)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (SirutaCodes.Counties.TryGetValue(birthPlace, out string county))
                BirthPlace = county;
            else
                BirthPlace = "????";
        }
    }
}
