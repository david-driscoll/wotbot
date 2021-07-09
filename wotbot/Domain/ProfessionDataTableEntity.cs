using Rocket.Surgery.Encoding;

namespace wotbot.Domain
{
    public class ProfessionDataTableEntity : BaseTableEntity
    {
        public string Profession { get; set; }
        public string Player { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public override string PartitionKey
        {
            get => Profession;
            set => Profession = value;
        }

        public override string RowKey
        {
            get => Base3264Encoding.ToBase32Crockford(Player + ":" + ItemId);
            set
            {
                var data = Base3264Encoding.FromBase32CrockfordToString(value).Split(":");
                Player = data[0];
                ItemId = int.Parse(data[1]);
            }
        }
    }
}