using ocpa.ro.api.Extensions;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Models.Generic
{
    public class StringAsBase64Converter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToBase64());
        }
    }

    public class NumberTruncateJsonConverter<T> : JsonConverter<T>
    {
        public const int DecimalPlaces = 2;

        private readonly int _decimalPlaces;

        public NumberTruncateJsonConverter() : this(DecimalPlaces)
        {
        }

        public NumberTruncateJsonConverter(int decimalPlaces)
        {
            _decimalPlaces = Math.Min(8, Math.Max(0, decimalPlaces));
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return (T)(object)reader.GetDouble();
            }
            catch
            {
                // Ignore the exception
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var val = (double)(object)value;
            writer.WriteRawValue((val).ToString($"F{_decimalPlaces}", CultureInfo.InvariantCulture));
        }
    }
}
