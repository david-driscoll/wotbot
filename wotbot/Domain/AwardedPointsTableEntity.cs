using System;
using Azure;
using Azure.Data.Tables;

namespace wotbot.Domain
{
    public class AwardedPointsTableEntity : BaseTableEntity
    {
        public string TeamId { get; set; } = null!;
        public string Player { get; set; } = null!;
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
            get => $"{Player}:{Index}";
            set
            {
                var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);
                Player = parts[0];
                Index = parts[1];
            }
        }
    }
}
