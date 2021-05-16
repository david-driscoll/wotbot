using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;

namespace wotbot.Workers
{
    class DiscordBotService : BackgroundService
    {
        private readonly DiscordClient _discordClient;
        private readonly DiscordRestClient _discordRestClient;

        public DiscordBotService(DiscordClient discordClient, DiscordRestClient discordRestClient)
        {
            _discordClient = discordClient;
            _discordRestClient = discordRestClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _discordClient.ConnectAsync();

            // var guilds = Observable.FromAsync(_ => _discordRestClient.GetCurrentUserGuildsAsync(200))
            //     .Select(results => (total: 0, results))
            //     .Expand(data =>
            //     {
            //         var (total, results) = data;
            //         if (results.Count == 0)
            //         {
            //             return Observable.Empty<ValueTuple<int, IReadOnlyList<DiscordGuild>>>();
            //         }
            //
            //         return Observable.FromAsync(_ => _discordRestClient.GetCurrentUserGuildsAsync(100, after: results.Last().Id))
            //             .Select(r => (total: total + results.Count, r));
            //     })
            //     .SelectMany(z => z.results)
            //     .Subscribe(x =>
            //     {
            //         Console.WriteLine(JsonSerializer.Serialize(x, new JsonSerializerOptions()
            //         {
            //             WriteIndented = true
            //         }));
            //     });

            await stoppingToken.WaitUntilCancelled();
        }
    }
}
