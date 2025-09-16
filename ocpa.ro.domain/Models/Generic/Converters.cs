using ocpa.ro.common.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Models.Generic
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
}
