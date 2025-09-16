using ocpa.ro.common;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Models.Meteo
{
    public class CalendarRange
    {
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Start { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime End { get; set; }

        public int Length { get; set; }
    }

    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public CustomDateTimeConverter()
        {
        }
        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
        {
            writer.WriteStringValue(date.ToString(Constants.DateFormat));
        }
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString(), Constants.DateFormat, CultureInfo.InvariantCulture);
        }
    }
}