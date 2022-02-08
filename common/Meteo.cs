using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    public class DateConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                var value = token.Value<string>();
                if (value.Length > 13)
                {
                    value = value.Remove(13);
                }
                return DateTime.ParseExact(value, MeteoConstants.DateFormat, CultureInfo.InvariantCulture);
                //return token.ToObject<DateTime>();
            }
            return DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            var str = value.ToString(MeteoConstants.DateFormat, CultureInfo.InvariantCulture);
            serializer?.Serialize(writer, str);
        }
    }


    public class CalendarRange
    {
        [JsonConverter(typeof(DateConverter))]
        [JsonProperty("Start")]
        public DateTime Start { get; set; }

        [JsonConverter(typeof(DateConverter))]
        [JsonProperty("End")]
        public DateTime End { get; set; }

        [JsonProperty("Length")]
        public int Length { get; set; }
    }

    public class GridCoordinates
    {
        [JsonProperty("R")]
        public int R { get; set; }

        [JsonProperty("C")]
        public int C { get; set; }
    }

    public class MeteoData
    {
        [JsonProperty("GridCoordinates")]
        public GridCoordinates GridCoordinates { get; set; }

        [JsonProperty("CalendarRange")]
        public CalendarRange CalendarRange { get; set; }

        [JsonProperty("Data")]
        public Dictionary<string, MeteoDailyData> Data { get; set; }
    }

    public class MeteoDailyData
    {
        [JsonProperty("TMinActual")]
        public string TMinActual { get; set; }

        [JsonProperty("TMaxActual")]
        public string TMaxActual { get; set; }

        [JsonProperty("TMinNormal")]
        public string TMinNormal { get; set; }

        [JsonProperty("TMaxNormal")]
        public string TMaxNormal { get; set; }

        [JsonProperty("Forecast")]
        public string Forecast { get; set; }

        [JsonProperty("TempFeel")]
        public string TempFeel { get; set; }
    }


    public static class MeteoConstants
    {
        public const string DateFormat = "yyyy-MM-dd";
    }
}
