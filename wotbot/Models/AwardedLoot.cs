using System;
using System.Text.Json.Serialization;

namespace wotbot.Models
{
    public record AwardedLoot(
        string Boss,
        [property: JsonPropertyName("loot")] string ItemLink,
        [property: JsonPropertyName("deletes")]
        string? DeletesIndex,
        [property: JsonPropertyName("deletedby")]
        string? DeletedBy,
        [property: JsonConverter(typeof(DateSecondsToDateTimeOffsetJsonConverter))]
        DateTimeOffset Date,
        string Index,
        long Cost,
        string Player
    );
}
