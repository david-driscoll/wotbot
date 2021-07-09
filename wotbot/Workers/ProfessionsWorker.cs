using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rocket.Surgery.DependencyInjection;
using wotbot.Domain;
using wotbot.Infrastructure;
using wotbot.Operations;

namespace wotbot
{
    class ProfessionsWorker : BackgroundService
    {
        private readonly DiscordClient _discordClient;
        private readonly IOptions<DiscordOptions> _options;
        private readonly ILogger<ProfessionsWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBlobContainerClientFactory _containerClientFactory;
        private readonly IExecuteScoped<IMediator> _executeScoped;

        private readonly BlockingCollection<AttachmentQueueItem> _attachmentQueue;
        private readonly BlockingCollection<ContentQueueItem> _contentQueue;
        private readonly BlockingCollection<CrafterQueue> _crafterReplyQueue;

        record AttachmentQueueItem(MessageCreateEventArgs Args, DiscordAttachment DiscordAttachment, DiscordMessage Response);

        record CrafterQueue(int ItemId, DiscordMessage Response);

        record CrafterQueueSpell(int ItemId, DiscordMessage Response) : CrafterQueue(ItemId, Response);

        record CrafterQueueItem(int ItemId, DiscordMessage Response) : CrafterQueue(ItemId, Response);

        record ContentQueueItem(string Content)
        {
            public DiscordMessage? Response { get; init; }
        }

        public ProfessionsWorker(DiscordClient discordClient, IOptions<DiscordOptions> options, ILogger<ProfessionsWorker> logger, IHttpClientFactory httpClientFactory,
            IBlobContainerClientFactory containerClientFactory, IExecuteScoped<IMediator> executeScoped)
        {
            _discordClient = discordClient;
            _options = options;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _containerClientFactory = containerClientFactory;
            _executeScoped = executeScoped;

            _attachmentQueue = new BlockingCollection<AttachmentQueueItem>();
            _contentQueue = new BlockingCollection<ContentQueueItem>();
            _crafterReplyQueue = new BlockingCollection<CrafterQueue>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Factory.StartNew(() => IngestAttachment(stoppingToken), stoppingToken);
            Task.Factory.StartNew(() => IngestContent(stoppingToken), stoppingToken);
            Task.Factory.StartNew(() => IngestCrafterSearch(stoppingToken), stoppingToken);

            _discordClient.MessageCreated += async (client, args) =>
            {
                if (
                    args.Author.Username != "Wowhead Bot" ||
                    _options.Value.SupportedGuilds.Any() && !_options.Value.SupportedGuilds.Contains(args.Guild.Name) ||
                    !_options.Value.ProfessionChannels.Contains(args.Channel.Name)
                )
                {
                    return;
                }

                var itemIdFragment = args.Message.Embeds
                    .Where(z => z.Url != null)
                    .Select(z => z.Url)
                    .FirstOrDefault()
                    ?.ToString()
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(z => z.StartsWith("item=") || z.StartsWith("spell="));
                if (string.IsNullOrWhiteSpace(itemIdFragment))
                {
                    return;
                }

                if (!int.TryParse(itemIdFragment[(itemIdFragment.IndexOf('=') + 1)..], out var itemId))
                {
                    return;
                }

                if (itemIdFragment.Contains("spell"))
                {
                    _crafterReplyQueue.Add(new CrafterQueueSpell(itemId, args.Message));
                }
                else
                {
                    _crafterReplyQueue.Add(new CrafterQueueItem(itemId, args.Message));
                }
            };

            _discordClient.MessageCreated += async (client, args) =>
            {
                if (
                    args.Author.Username == "Wowhead Bot" ||
                    _options.Value.SupportedGuilds.Any() && !_options.Value.SupportedGuilds.Contains(args.Guild.Name) ||
                    !_options.Value.ProfessionChannels.Contains(args.Channel.Name)
                )
                {
                    return;
                }

                if (args.Message.Content.Contains("!profession import"))
                {
                    _contentQueue.Add(new ContentQueueItem(args.Message.Content) { Response = args.Message });
                }

                if (args.Message.Attachments.Any())
                {
                    foreach (var atttachment in args.Message.Attachments)
                    {
                        _attachmentQueue.Add(new AttachmentQueueItem(args, atttachment, args.Message));
                    }
                }
            };

            return stoppingToken.WaitUntilCancelled();
        }

        private async Task IngestAttachment(CancellationToken cancellationToken)
        {
            while (_attachmentQueue.TryTake(out var data, -1, cancellationToken))
            {
                var (args, attachment, response) = data;
                using var httpClient = _httpClientFactory.CreateClient(nameof(SavedVariablesWorker));
                var documentResponse = await httpClient.GetStringAsync(attachment.Url, cancellationToken);
                _contentQueue.Add(new ContentQueueItem(documentResponse) { Response = response }, cancellationToken);
            }
        }

        private static Regex IdRegex = new Regex(@"""id"":(?<id>\d+)");
        private static Regex NameRegex = new Regex(@"""name"":""(?<name>.+?)""");

        private async Task IngestContent(CancellationToken cancellationToken)
        {
            while (_contentQueue.TryTake(out var data, -1, cancellationToken))
            {
                var content = data.Content;
                var professionData = await _executeScoped.Invoke((m, ct) => m.Send(new ExtractProfessionDataFromMessage.Request(content), ct), cancellationToken);
                foreach (var row in professionData)
                {
                    var items = row.Items.Select(z => new UploadProfessionData.Item(z.Id, z.Name)).ToList();
                    if (row.Profession.Equals("Enchanting", StringComparison.OrdinalIgnoreCase))
                    {
                        var client = new HttpClient();
                        foreach (var item in items.ToArray())
                        {
                            var spellId = item.Id;
                            var whContent = await client.GetStringAsync($"https://tbc.wowhead.com/spell={spellId}", cancellationToken);
                            var lines = whContent.Split('\n');
                            var line = lines.FirstOrDefault(z => z.Contains("WH.TERMS.taughtby"));
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var idMatch = IdRegex.Match(line);
                            if (!idMatch.Success) continue;
                            var nameMatch = NameRegex.Match(line, idMatch.Index);
                            if (!nameMatch.Success || !nameMatch.Groups["name"].Value.StartsWith("Formula:")) continue;

                            items.Add(new UploadProfessionData.Item(int.Parse(idMatch.Groups["id"].Value), nameMatch.Groups["name"].Value));
                            //WH.TERMS.taughtby
                        }
                    }

                    await _executeScoped.Invoke((m, ct) => m.Send(new UploadProfessionData.Request(row.Player, row.Profession, items), ct), cancellationToken);
                }

                if (data.Response is { })
                {
                    await data.Response.DeleteAsync();
                }
            }
        }

        private async Task IngestCrafterSearch(CancellationToken cancellationToken)
        {
            while (_crafterReplyQueue.TryTake(out var data, -1, cancellationToken))
            {
                var crafters = (await _executeScoped.Invoke((m, ct) => m.Send(new FindCrafters.Request(data.ItemId), ct))).ToArray();
                if (crafters.Any())
                {
                    await data.Response.RespondAsync(new DiscordMessageBuilder()
                        .WithEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Crafters")
                            .WithColor(DiscordColor.PhthaloGreen)
                            .WithDescription(string.Join(", ", crafters))
                        )
                    );
                }
                else
                {
                    await data.Response.RespondAsync(new DiscordMessageBuilder()
                        .WithEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Crafters")
                            .WithColor(DiscordColor.Yellow)
                            .WithDescription("No known crafters")
                        )
                    );
                }
            }
        }
    }
}
