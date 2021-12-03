using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rocket.Surgery.DependencyInjection;
using wotbot.Infrastructure;
using wotbot.Models;
using wotbot.Operations;
using Humanizer;

namespace wotbot
{
    class SavedVariablesWorker : BackgroundService
    {
        private readonly DiscordClient _discordClient;
        private readonly IOptions<DiscordOptions> _options;
        private readonly ILogger<SavedVariablesWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBlobContainerClientFactory _containerClientFactory;
        private readonly IExecuteScoped<IMediator> _executeScoped;
        private readonly BlockingCollection<AttachmentQueueItem> _attachmentQueue;
        private readonly BlockingCollection<VariableQueueItem> _variablesQueue;

        record AttachmentQueueItem(MessageCreateEventArgs Args, DiscordAttachment DiscordAttachment, DiscordMessage Response);

        record VariableQueueItem(string ContainerName, string BlobPath)
        {
            public DiscordMessage? Response { get; init; }
        }

        public SavedVariablesWorker(DiscordClient discordClient, IOptions<DiscordOptions> options, ILogger<SavedVariablesWorker> logger, IHttpClientFactory httpClientFactory,
            IBlobContainerClientFactory containerClientFactory, IExecuteScoped<IMediator> executeScoped)
        {
            _discordClient = discordClient;
            _options = options;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _containerClientFactory = containerClientFactory;
            _executeScoped = executeScoped;

            _variablesQueue = new BlockingCollection<VariableQueueItem>();
            _attachmentQueue = new BlockingCollection<AttachmentQueueItem>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Factory.StartNew(() => IngestFiles(stoppingToken), stoppingToken);
            Task.Factory.StartNew(() => IngestAttachment(stoppingToken), stoppingToken);

            _discordClient.MessageCreated += async (client, args) =>
            {
                if (
                    _options.Value.SupportedGuilds.Any() && !_options.Value.SupportedGuilds.Contains(args.Guild.Name) ||
                    !_options.Value.SavedVariablesChannels.Contains(args.Channel.Name)
                )
                {
                    return;
                }

                foreach (var attachment in args.Message.Attachments)
                {
                    var response = await args.Message.RespondAsync("I'll get right on it!");
                    OnAttachment(new AttachmentQueueItem(args, attachment, response));
                }
            };

            return stoppingToken.WaitUntilCancelled();
        }

        private async Task IngestAttachment(CancellationToken cancellationToken)
        {
            while (_attachmentQueue.TryTake(out var data, -1, cancellationToken))
            {
                try
                {
                    var (args, attachment, response) = data;
                    using var httpClient = _httpClientFactory.CreateClient(nameof(SavedVariablesWorker));
                    var documentResponse = await httpClient.GetStreamAsync(attachment.Url, cancellationToken);
                    var containerClient = _containerClientFactory.CreateClient(Constants.SavedVariablesStaging);
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
                    var path =
                        $"{args.Guild.Id}/{attachment.CreationTimestamp:yyyy-MM-dd}/{args.Author.Username}/{args.Message.Id}-{attachment.Id}-{Path.GetFileName(attachment.FileName)}";
                    await containerClient.UploadBlobAsync(
                        $"{args.Guild.Id}/{attachment.CreationTimestamp:yyyy-MM-dd}/{args.Author.Username}/{args.Message.Id}-{attachment.Id}-{Path.GetFileName(attachment.FileName)}",
                        documentResponse,
                        cancellationToken
                    );
                    _variablesQueue.Add(new VariableQueueItem(Constants.SavedVariablesStaging, path) { Response = response }, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed processing Attachment");
                }
            }
        }

        private async Task IngestFiles(CancellationToken cancellationToken)
        {
            while (_variablesQueue.TryTake(out var data, -1, cancellationToken))
            {
                try
                {
                    var response = await _executeScoped.Invoke(x => x.Send(new ExtractDataFromSavedVariables.Request(data.ContainerName, data.BlobPath), cancellationToken));
                    // _logger.LogInformation("Ingest {@Data}", response);

                    if (data.Response is not null)
                    {
                        await data.Response.ModifyAsync(new DiscordMessageBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .AddField("Status", "Processing")
                            )
                        );
                    }

                    // handler per team points / loot
                    await _executeScoped.Invoke(x => x.Send(new UploadTeams.Request(response.Keys.ToImmutableArray()), cancellationToken));
                    var results = await response
                        .ToAsyncEnumerable()
                        .SelectAwaitWithCancellation(async (d, ct ) =>
                        {
                            var playerProfileUpload = _executeScoped.Invoke(x => x.Send(new UploadPlayerProfiles.Request(d.Key, d.Value.PlayerProfiles), ct));
                            var awardedPointsUpload = _executeScoped.Invoke(x => x.Send(new UploadAwardedPoints.Request(d.Key, d.Value.AwardedPoints), ct));
                            var awardedLootUpload = _executeScoped.Invoke(x => x.Send(new UploadAwardedLoot.Request(d.Key, d.Value.AwardedLoot), ct));
                            await Task.WhenAll(playerProfileUpload, awardedLootUpload, awardedPointsUpload);
                            return (team: d.Key, newLoot: awardedLootUpload.Result, newPoints: awardedPointsUpload.Result);
                        })
                        .ToArrayAsync(cancellationToken);
                    // _logger.LogInformation("Finished Ingest {@Data}", response);

                    if (data.Response is not null)
                    {
                        var totalLootRecords = results.Aggregate(0, (a, b) => b.newLoot.Length + a);
                        var totalAwardRecords = results.Aggregate(0, (a, b) => b.newPoints.Length + a);
                        await data.Response.ModifyAsync(new DiscordMessageBuilder()
                            .WithEmbed(new DiscordEmbedBuilder()
                                .AddField("Status", "Finished")
                                .AddField("Updated Teams", response.Count.ToString(), false)
                                .AddField("Updated Teams", response.Count.ToString(), true)
                                .AddField("New Loot Records", totalLootRecords.ToString(), true)
                                .AddField("New Points Records", totalAwardRecords.ToString(), true)
                            )
                        );

                        if (data.Response?.Channel?.GuildId is { } guildId)
                        {
                            var g = _discordClient.Guilds[guildId];

                            var reportChannels = g.Channels.Values.Where(z => _options.Value.OutputChannels.Contains(z.Name));
                            var initialMessage = new DiscordMessageBuilder()
                                .WithEmbed(
                                    new DiscordEmbedBuilder()
                                        .WithTitle("New loot added!")
                                        .WithDescription($"Uploaded by {data.Response.ReferencedMessage?.Author?.Mention}")
                                );

                            var supportedResults = results.Where(z => _options.Value.SupportedTeams.Contains(z.team.TeamId));
                            var supportedTeams = supportedResults.Select(z => z.team).Distinct();
                            var lootMessages = await supportedResults
                                .ToAsyncEnumerable()
                                .SelectMany(z => GetLootMessages(z.team, z.newLoot))
                                .ToArrayAsync(cancellationToken);

                            var standingMessages = await supportedTeams
                                .ToAsyncEnumerable()
                                .SelectMany(GetStandingsMessage)
                                .ToArrayAsync(cancellationToken);

                            async IAsyncEnumerable<DiscordMessageBuilder> GetStandingsMessage(TeamRecord team)
                            {
                                var standings = await _executeScoped.Invoke((x, ct) => x.Send(new ListProfiles.Request(team.TeamId), ct), cancellationToken);
                                yield return new DiscordMessageBuilder()
                                    .WithEmbed(
                                        new DiscordEmbedBuilder()
                                            .WithTitle($"Standings {team.Name}")
                                            .AddStandings(g, standings));
                            }

                            async IAsyncEnumerable<DiscordMessageBuilder> GetLootMessages(TeamRecord team, ImmutableArray<AwardedLoot> awardedLoots)
                            {
                                foreach (var playerLoot in awardedLoots.GroupBy(z => z.Player))
                                {
                                    var profile = await _executeScoped.Invoke((z, ct) => z.Send(new GetPlayerProfile.Request(team.TeamId, playerLoot.Key), ct), cancellationToken);

                                    var items = string.Join("\n", playerLoot.Select(z =>
                                    {
                                        var itemLink = ItemLink.Parse(z.ItemLink);
                                        if (itemLink is null) return "";
                                        return $"[{itemLink.ItemName}](https://tbc.wowhead.com/item={itemLink.ItemInfo.ItemId}) @ {z.Cost}";
                                    }));
                                    yield return new DiscordMessageBuilder()
                                        .WithEmbed(new DiscordEmbedBuilder()
                                            .WithTitle(profile.Player)
                                            .WithDescription(@$"
Items Won:

{items}
")
                                            .WithColor(profile.GetClassColor())
                                            .WithFooter(profile.GetFullSpec(),
                                                iconUrl:
                                                $"https://wotbot.azurewebsites.net/spec/{profile.GetClassName().Dasherize().ToLowerInvariant()}/{profile.GetSpecName().Dasherize().ToLowerInvariant()}.png")
                                            .WithThumbnail($"https://wotbot.azurewebsites.net/anime/{profile.GetClassName().Dasherize().ToLowerInvariant()}.png")
                                        );
                                }
                            }

                            foreach (var channel in reportChannels)
                            {
                                await channel.SendMessageAsync(initialMessage);
                            }
                            if (lootMessages.Any() || standingMessages.Any())
                            {
                                foreach (var channel in reportChannels)
                                {
                                    foreach (var message in lootMessages)
                                    {
                                        await channel.SendMessageAsync(message);
                                    }
                                    foreach (var message in standingMessages)
                                    {
                                        await channel.SendMessageAsync(message);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error ingesting file {ContainerName}{FilePath}", data.ContainerName, data.BlobPath);
                    if (data is { Response: { } r })
                    {
                        await data.Response.ModifyAsync(new DiscordMessageBuilder()
                            .WithContent($"Error ingesting file {e.Message} {e.StackTrace}".Truncate(2000))
                            .WithEmbed(new DiscordEmbedBuilder()
                                .AddField("Status", "Failed")
                            )
                        );
                    }
                }
            }
        }

        private void OnBlobCreated(VariableQueueItem item)
        {
            _variablesQueue.Add(item);
        }

        private void OnAttachment(AttachmentQueueItem item)
        {
            _attachmentQueue.Add(item);
        }
    }
}
