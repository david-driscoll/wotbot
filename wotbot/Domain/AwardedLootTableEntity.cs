using System;

namespace wotbot.Domain
{
    public class AwardedLootTableEntity : BaseTableEntity
    {
        public string TeamId { get; set; } = null!;
        public string Boss { get; set; } = null!;
        public string ItemLink { get; set; } = null!;
        public string? DeletesIndex { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Index { get; set; } = null!;
        public long Cost { get; set; }
        public string Player { get; set; } = null!;

        public override string PartitionKey
        {
            get => TeamId;
            set => TeamId = value;
        }

        public override string RowKey
        {
            get => Index;
            set => Index = value;
        }
    }
}
