using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GetRaidPointsCommand> _logger;

        public GetRaidPointsCommand(IExecuteScoped<IMediator> executeScoped, IOptions<DiscordOptions> options, ILogger<GetRaidPointsCommand> logger)
        {
            _executeScoped = executeScoped;
            _options = options;
            _logger = logger;
            _logger.LogInformation("Commands are setup");
        }

        [Command("rp")]
        public async Task GetRaidPoints(CommandContext ctx)
        {
            try
            {
                var team = _options.Value.SupportedTeams.Single();
                var profile = await _executeScoped.Invoke((x, ct) => x.Send(new GetPlayerProfile.Request(team, ctx.User.Username), ct));

                var msg = new DiscordMessageBuilder()
                    .WithEmbed(CreateProfileEmbed(profile)
                        .WithDescription($"{ctx.User.Mention} you have {profile.CurrentPoints} points!")
                    )
                    .WithAllowedMention(new UserMention(ctx.User));
                await ctx.RespondAsync(msg);
            }
            catch (NotFoundException)
            {
                var msg = new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        // .WithTitle("WOT BOT")
                        .WithDescription($"{ctx.User.Mention}, I can't find any points for you! Maybe try !rp <character>.")
                        .WithColor(DiscordColor.Gray)
                        .WithThumbnail($"https://wotbot.azurewebsites.net/question.gif")
                    ).WithAllowedMention(new UserMention(ctx.User));
                await ctx.RespondAsync(msg);
            }
            catch (Exception e)
            {
                var msg = new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        // .WithTitle("WOT BOT")
                        .WithDescription($"Oh boy, something went really wrong!!")
                        .WithFooter(e.Message)
                        .WithColor(DiscordColor.Red)
                    );
                await ctx.RespondAsync(msg);
            }
        }

        [Command("rp")]
        public async Task GetRaidPoints(CommandContext ctx, string name)
        {
            try
            {
                var team = _options.Value.SupportedTeams.Single();
                var profile = await _executeScoped.Invoke((x, ct) => x.Send(new GetPlayerProfile.Request(team, name), ct));
                var msg = new DiscordMessageBuilder()
                    .WithEmbed(
                        CreateProfileEmbed(profile)
                            .WithDescription($"_{profile.Player}_ has **{profile.CurrentPoints}** points!")
                    );
                await ctx.RespondAsync(msg);
            }
            catch (NotFoundException)
            {
                var msg = new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        // .WithTitle("WOT BOT")
                        .WithDescription($"I'm sorry I could not find any points for _{name}_!")
                        .WithColor(DiscordColor.Gray)
                        .WithThumbnail($"https://wotbot.azurewebsites.net/question.gif")
                    );
                await ctx.RespondAsync(msg);
            }
            catch (Exception e)
            {
                var msg = new DiscordMessageBuilder()
                    .WithEmbed(new DiscordEmbedBuilder()
                        // .WithTitle("WOT BOT")
                        .WithDescription($"Oh boy, something went really wrong!!")
                        .WithFooter(e.Message)
                        .WithColor(DiscordColor.Red)
                    );
                await ctx.RespondAsync(msg);
            }
        }

        [Command("dkp")]
        public Task GetDkp(CommandContext ctx) => GetRaidPoints(ctx);

        [Command("dkp")]
        public Task GetDkp(CommandContext ctx, string name) => GetRaidPoints(ctx, name);

        private DiscordEmbedBuilder CreateProfileEmbed(PlayerProfile profile)
        {
            return new DiscordEmbedBuilder()
                    // .WithTitle("WOT BOT")
                    // .AddField("Rank", profile.Rank, false)
                    .AddField("Lifetime", profile.LifetimePoints.ToString("D"), true)
                    .AddField("Spent", profile.LifetimeSpent.ToString("D"), true)
                    .WithColor(profile.GetClassColor())
                    .WithFooter(profile.GetFullSpec(), iconUrl: $"https://wotbot.azurewebsites.net/spec/{profile.GetClassName().Dasherize().ToLowerInvariant()}/{profile.GetSpecName().Dasherize().ToLowerInvariant()}.png")
                    .WithThumbnail($"https://wotbot.azurewebsites.net/anime/{profile.GetClassName().Dasherize().ToLowerInvariant()}.png");
        }
    }
}
