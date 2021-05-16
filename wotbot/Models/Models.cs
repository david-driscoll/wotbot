using System.Text.Json.Serialization;

namespace wotbot.Models
{
    public record AwardedLoot(
        string Boss,
        [property: JsonPropertyName("loot")] string ItemLink,
        [property: JsonPropertyName("deletes")] string? DeletesIndex,
        [property: JsonPropertyName("deletedby")] string? DeletedBy,
        long Date,
        string Index,
        long Cost,
        string Player
    );

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
        long Date,
        string Reason
    );
}
