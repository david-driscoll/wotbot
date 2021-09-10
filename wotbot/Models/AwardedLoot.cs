using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using NLua;

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

    public record Bidder(string Name, long Dkp);
}
