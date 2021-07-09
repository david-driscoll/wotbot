using System;
using System.Text.Json.Serialization;

namespace wotbot.Models
{
    public record AwardedPoints(
        string Player,
        string Index,
        [property: JsonPropertyName("dkp")] long Points,
        [property: JsonConverter(typeof(DateSecondsToDateTimeOffsetJsonConverter))]
        DateTimeOffset Date,
        string Reason
    );
}