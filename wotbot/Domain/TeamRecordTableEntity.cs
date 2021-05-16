using System;
using Azure;
using Azure.Data.Tables;

namespace wotbot.Domain
{
    public class TeamRecordTableEntity : BaseTableEntity
    {
        public string TeamId => $"{Server}-{Faction}-{Guild}-{Index}";
        public string Server { get; set; }= null!;
        public string Faction { get; set; }= null!;
        public string Guild { get; set; }= null!;
        public string Index { get; set; }= null!;
        public string Name { get; set; }= null!;

        public override string PartitionKey
        {
            get => Server;
            set => Server = value;
        }

        public override string RowKey
        {
            get => TeamId;
            set
            {
                var result = TeamId.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (result is {Length: > 0}) Server = result[0];
                if (result is {Length: > 1}) Faction = result[1];
                if (result is {Length: > 2}) Guild = result[2];
                if (result is {Length: > 3}) Index = result[3];
            }
        }
    }
}
