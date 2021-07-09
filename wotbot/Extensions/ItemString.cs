using System.Text.RegularExpressions;

namespace wotbot
{
    public record ItemString(int ItemId, int? EnchantId, int? GemId1, int? GemId2, int? GemId3, int? GemId4, int? SuffixId, int? UniqueId, int? LinkLevel, int? SpecializationId,
        int? UpgradeId, int? InstanceDifficultyId, int? NumBonusIds, int? BonusId1, int? BonusId2, int? UpgradeValue)
    {
        private static readonly Regex ItemStringRegex =
            new(
                @"item:(?<itemId>\d+):(?<enchantId>\d*):(?<gemId1>\d*):(?<gemId2>\d*):(?<gemId3>\d*):(?<gemId4>\d*):(?<suffixId>\d*):(?<uniqueId>\d*):(?<linkLevel>\d*):(?<specializationId>\d*):(?<upgradeId>\d*):(?<instanceDifficultyId>\d*):(?<numBonusIds>\d*):(?<bonusId1>\d*):(?<bonusId2>\d*):(?<upgradeValue>\d*):(?<unknown>\d*)"
                , RegexOptions.Compiled);

        public static ItemString? Parse(string itemLink)
        {
            var match = ItemStringRegex.Match(itemLink);
            if (!match.Success) return null;

            int temp;
            return new ItemString(
                int.Parse(match.Groups["itemId"].Value),
                int.TryParse(match.Groups["enchantId"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["gemId1"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["gemId2"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["gemId3"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["gemId4"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["suffixId"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["uniqueId"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["linkLevel"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["specializationId"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["upgradeId"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["instanceDifficultyId"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["numBonusIds"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["bonusId1"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["bonusId2"].Value, out temp) ? temp : null,
                int.TryParse(match.Groups["upgradeValue"].Value, out temp) ? temp : null
            );
        }
    }
}