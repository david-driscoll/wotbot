namespace wotbot.Models
{
    public sealed record TeamRecord(string Server, string Faction, string Guild, string Index, string Name)
    {
        public string TeamId { get; } = $"{Server}-{Faction}-{Guild}-{Index}";
        public override int GetHashCode() => TeamId.GetHashCode();
        public bool Equals(TeamRecord? other) => TeamId == other?.TeamId;
    }
}