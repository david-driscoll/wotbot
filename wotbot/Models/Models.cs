using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace wotbot.Models
{
    public record AwardedLoot(
        string Boss,
        [property: JsonPropertyName("loot")] string ItemLink,
        [property: JsonPropertyName("deletes")] string? DeletesIndex,
        [property: JsonPropertyName("deletedby")] string? DeletedBy,
        [property: JsonConverter(typeof(DateSecondsToDateTimeOffsetJsonConverter))]
        DateTimeOffset Date,
        string Index,
        long Cost,
        string Player
    );

    class DateSecondsToDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset> {
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

    public record PlayerProfile(
        [property: JsonPropertyName("previous_dkp")] long PreviousPoints,
        [property: JsonPropertyName("dkp")] long CurrentPoints,
        [property: JsonPropertyName("lifetime_gained")] long LifetimePoints,
        [property: JsonPropertyName("lifetime_spent")] long LifetimeSpent,
        [property: JsonPropertyName("version")] string Version,
        [property: JsonPropertyName("class")] string Class,
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("spec")] string Spec,
        [property: JsonPropertyName("rankName")] string RankName,
        long Rank,
        string Player
    );

    public record AwardedPoints(
        string Player,
        string Index,
        [property: JsonPropertyName("dkp")] long Points,
        [property: JsonConverter(typeof(DateSecondsToDateTimeOffsetJsonConverter))]
        DateTimeOffset Date,
        string Reason
    );

    public sealed record TeamRecord(string Server, string Faction, string Guild, string Index, string Name)
    {
        public string TeamId { get; } = $"{ Server}-{Faction}-{Guild}-{Index}";
        public override int GetHashCode() => TeamId.GetHashCode();
        public bool Equals(TeamRecord? other) => TeamId == other?.TeamId;
    }
}
