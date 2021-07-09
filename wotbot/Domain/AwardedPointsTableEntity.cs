using System;
using System.Globalization;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using Rocket.Surgery.Encoding;

namespace wotbot.Domain
{
    public class AwardedPointsTableEntity : BaseTableEntity
    {
        public string TeamId { get; set; } = null!;
        public string Player { get; set; } = null!;
        public string? PlayerSafe { get; set; }
        public string Index { get; set; } = null!;
        public long Points { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Reason { get; set; } = null!;

        public override string PartitionKey
        {
            get => TeamId;
            set => TeamId = value;
        }

        public override string RowKey
        {
            get => Base3264Encoding.ToBase32Crockford($"{Player}:{Index}");
            set
            {
                var parts = Base3264Encoding.FromBase32CrockfordToString(value).Split(':', StringSplitOptions.RemoveEmptyEntries);
                Index = parts[1];
                PlayerSafe = parts[0];
            }
        }
    }
}
