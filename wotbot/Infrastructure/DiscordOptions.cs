using System.Collections.Generic;

namespace wotbot.Infrastructure
{
    public class DiscordOptions
    {
        public string Token { get; init; } = null!;
        public string ClientId { get; init; } = null!;
        public string ClientSecret { get; init; } = null!;
        public string CommandPrefix { get; init; } = null!;

        public HashSet<string> SupportedGuilds { get; init; } = new ();
        public HashSet<string> SupportedTeams { get; init; } = new ();
        public HashSet<string> SavedVariablesChannels { get; init; } = new ();
        public HashSet<string> OutputChannels { get; init; } = new ();
        public HashSet<string> ProfessionChannels { get; init; } = new ();
    }
}
