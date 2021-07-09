using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace wotbot.Models
{
    class DateSecondsToDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var l = JsonSerializer.Deserialize<long>(ref reader, options);
            return DateTimeOffset.FromUnixTimeSeconds(l);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToUnixTimeSeconds(), options);
        }
    }
}