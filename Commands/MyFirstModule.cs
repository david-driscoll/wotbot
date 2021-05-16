using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace wotbot.Commands
{
    public class MyFirstModule : BaseCommandModule
    {
        [Command("rp")]
        public async Task GreetCommand(CommandContext ctx)
        {
            await ctx.RespondAsync($"Go find data...for...{ctx.User.Username}");
        }

        [Command("rp")]
        public async Task GreetCommand(CommandContext ctx, string name)
        {
            await ctx.RespondAsync($"Go find data...for...{name}");
        }
    }
}
