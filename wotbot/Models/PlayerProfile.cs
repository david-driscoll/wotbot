using System.Text.Json.Serialization;
using DSharpPlus.Entities;
using Humanizer;

namespace wotbot.Models
{
    public record PlayerProfile(
        [property: JsonPropertyName("previous_dkp")]
        long PreviousPoints,
        [property: JsonPropertyName("dkp")] long CurrentPoints,
        [property: JsonPropertyName("lifetime_gained")]
        long LifetimePoints,
        [property: JsonPropertyName("lifetime_spent")]
        long LifetimeSpent,
        [property: JsonPropertyName("version")]
        string Version,
        [property: JsonPropertyName("class")] string Class,
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("spec")] string Spec,
        // [property: JsonPropertyName("rankName")]
        // string RankName,
        long Rank,
        string Player
    )
    {
        public string GetSpecName()
        {
            if (string.IsNullOrWhiteSpace(Spec))
                return "Unknown";
            var profileIndex = Spec.IndexOf('(');
            return profileIndex > -1 ? Spec![..profileIndex].Trim() : "Unknown";
        }

        public string GetFullSpec()
        {
            if (GetSpecName() is "Unknown") return "Unknown";
            return Spec;
        }

        public string GetClassName() => Class.Titleize();

        public WarcraftClass GetClass() =>
            GetClassName().Dasherize().ToLowerInvariant() switch
            {
                "death-knight" => WarcraftClass.DeathKnight,
                "demon-hunter" => WarcraftClass.DemonHunter,
                "druid" => WarcraftClass.Druid,
                "hunter" => WarcraftClass.Hunter,
                "mage" => WarcraftClass.Mage,
                "monk" => WarcraftClass.Monk,
                "paladin" => WarcraftClass.Paladin,
                "priest" => WarcraftClass.Priest,
                "rogue" => WarcraftClass.Rogue,
                "shaman" => WarcraftClass.Shaman,
                "warlock" => WarcraftClass.Warlock,
                "warrior" => WarcraftClass.Warrior,
                _ => WarcraftClass.Unknown
            };

        public WarcraftRole GetRole() =>
            GetClassName().Dasherize().ToLowerInvariant() switch
            {
                "caster-dps" => WarcraftRole.Caster,
                "range-dps" => WarcraftRole.Ranged,
                "melee-dps" => WarcraftRole.Melee,
                "healer" => WarcraftRole.Healer,
                "tank" => WarcraftRole.Tank,
                _ => WarcraftRole.Unknown
            };

        public DiscordColor GetClassColor() =>
            GetClass() switch
            {
                WarcraftClass.DeathKnight => new DiscordColor(196, 30, 58),
                WarcraftClass.DemonHunter => new DiscordColor(163, 48, 201),
                WarcraftClass.Druid => new DiscordColor(255, 124, 10),
                WarcraftClass.Hunter => new DiscordColor(170, 211, 114),
                WarcraftClass.Mage => new DiscordColor(63, 199, 235),
                WarcraftClass.Monk => new DiscordColor(0, 255, 152),
                WarcraftClass.Paladin => new DiscordColor(244, 140, 186),
                WarcraftClass.Priest => new DiscordColor(255, 255, 255),
                WarcraftClass.Rogue => new DiscordColor(255, 244, 104),
                WarcraftClass.Shaman => new DiscordColor(0, 112, 221),
                WarcraftClass.Warlock => new DiscordColor(135, 136, 238),
                WarcraftClass.Warrior => new DiscordColor(198, 155, 109),
                WarcraftClass.Unknown => DiscordColor.Gray,
                _ => DiscordColor.Gray
            };
    }
}