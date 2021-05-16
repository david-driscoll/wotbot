using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.LaunchPad.Foundation;
using wotbot.Infrastructure;
using wotbot.Models;
using wotbot.Operations;

namespace wotbot.Commands
{
    public class GetRaidPointsCommand : BaseCommandModule
    {
        private readonly IExecuteScoped<IMediator> _executeScoped;
        private readonly IOptions<DiscordOptions> _options;

        public GetRaidPointsCommand(IExecuteScoped<IMediator> executeScoped, IOptions<DiscordOptions> options)
        {
            _executeScoped = executeScoped;
            _options = options;
        }

        [Command("rp")]
        public async Task GetRaidPoints(CommandContext ctx)
        {
            try
            {
                var team = _options.Value.SupportedTeams.Single();
                var profile = await _executeScoped.Invoke((x, ct) => x.Send(new GetRaidPoints.Request(team, ctx.User.Username), ct));

                var msg = new DiscordMessageBuilder()
                    .WithContent($"{ctx.User.Mention} you have {profile.CurrentPoints} points!")
                    .WithAllowedMention(new UserMention(ctx.User));
                await ctx.RespondAsync(msg);
            }
            catch (NotFoundException)
            {
                var msg = new DiscordMessageBuilder()
                    .WithContent($"{ctx.User.Mention}, I can't find any points for you! Maybe try !rp <character>.")
                    .WithAllowedMention(new UserMention(ctx.User));
                await ctx.RespondAsync(msg);
            }
            catch
            {
                await ctx.RespondAsync($"Oh boy, something went really wrong!!");
            }
        }

        [Command("rp")]
        public async Task GetRaidPoints(CommandContext ctx, string name)
        {
            try
            {
                var team = _options.Value.SupportedTeams.Single();
                var profile = await _executeScoped.Invoke((x, ct) => x.Send(new GetRaidPoints.Request(team, name), ct));
                var msg = new DiscordMessageBuilder()

                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithTitle("WOT BOT")
                        .WithDescription($"_{profile.Player}_ has **{profile.CurrentPoints}** points!")
                        .WithColor(DiscordColor.Rose)
                        .WithFooter("Hello footer")
                        .WithThumbnail("http://www.dustball.com/icons/icons/accept.png")
                    );
                await ctx.RespondAsync(msg);
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync($"I'm sorry I could not find any points for {name}!");
            }
            catch
            {
                await ctx.RespondAsync($"Oh boy, something went really wrong!!");
            }
        }
    }
}
