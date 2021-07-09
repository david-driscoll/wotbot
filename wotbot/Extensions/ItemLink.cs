using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace wotbot
{
    public record ItemLink(DiscordColor Color, ItemString ItemInfo, string ItemName)
    {
        private static readonly Regex ItemLinkRegex =
            new(
                @"\|c(?:[0-9a-f]{2})?(?<color>[0-9a-f]{6})\|H(?<itemString>.*)\|h(?<itemName>.*?)\|h\|r", RegexOptions.Compiled);

        public static ItemLink? Parse(string itemLink)
        {
            var match = ItemLinkRegex.Match(itemLink);
            if (!match.Success) return null;

            var itemString = ItemString.Parse(match.Groups["itemString"].Value);
            if (itemString is null) return null;

            var color = match.Groups["color"].Value;
            var red = byte.Parse(color[..2], NumberStyles.HexNumber);
            var green = byte.Parse(color[2..4], NumberStyles.HexNumber);
            var blue = byte.Parse(color[4..], NumberStyles.HexNumber);
            return new ItemLink(
                new DiscordColor(red, green, blue),
                itemString,
                match.Groups["itemName"].Value
            );
        }
    }
}