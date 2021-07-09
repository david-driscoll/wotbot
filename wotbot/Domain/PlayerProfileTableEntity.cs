using System;
using System.Globalization;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using Rocket.Surgery.Encoding;

namespace wotbot.Domain
{
    public class PlayerProfileTableEntity : BaseTableEntity
    {
        public string TeamId { get; set; } = null!;
        public long PreviousPoints { get; set; }
        public long CurrentPoints { get; set; }
        public long LifetimePoints { get; set; }
        public long LifetimeSpent { get; set; }
        public string Version { get; set; } = null!;
        public string Class { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Spec { get; set; } = null!;
        // public string RankName { get; set; } = null!;
        public long Rank { get; set; }
        public string Player { get; set; } = null!;
        public string PlayerLower { get; set; } = null!;

        public override string PartitionKey
        {
            get => TeamId;
            set => TeamId = value;
        }

        public override string RowKey
        {
            get => Base3264Encoding.ToBase32Crockford(PlayerLower);
            set => PlayerLower = Base3264Encoding.FromBase32CrockfordToString(value);
        }
    }
}
