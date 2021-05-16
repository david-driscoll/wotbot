using System;
using System.Collections.Concurrent;
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
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rocket.Surgery.DependencyInjection;
using wotbot.Infrastructure;
using wotbot.Operations;

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

        record AttachmentQueueItem(MessageCreateEventArgs Args, DiscordAttachment discordAttachment, DiscordMessage Response);

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
                _logger.LogInformation("Response from {Guild}#{Channel}", args.Guild.Name, args.Channel.Name);
                if (
                    !_options.Value.SupportedGuilds.Contains(args.Guild.Name)
                    || !_options.Value.SavedVariablesChannels.Contains(args.Channel.Name)
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
                _variablesQueue.Add(new VariableQueueItem(Constants.SavedVariablesStaging, path) {Response = response}, cancellationToken);
            }
        }

        private async Task IngestFiles(CancellationToken cancellationToken)
        {
            while (_variablesQueue.TryTake(out var data, -1, cancellationToken))
            {
                try
                {
                    var response = await _executeScoped.Invoke(x => x.Send(new ExtractDataFromSavedVariables.Request(data.ContainerName, data.BlobPath), cancellationToken));
                    _logger.LogInformation("Ingest {@Data}", response);

                    await response.SelectMany(d => new[]
                        {
                            Observable.FromAsync(ct => _executeScoped.Invoke(x => x.Send(new UploadPlayerProfiles.Request(d.Key, d.Value.PlayerProfiles), ct))),
                            Observable.FromAsync(ct => _executeScoped.Invoke(x => x.Send(new UploadAwardedPoints.Request(d.Key, d.Value.AwardedPoints), ct))),
                            Observable.FromAsync(ct => _executeScoped.Invoke(x => x.Send(new UploadAwardedLoot.Request(d.Key, d.Value.AwardedLoot), ct)))
                        })
                        .ToObservable()
                        .Merge()
                        .Merge(Observable.FromAsync(ct => _executeScoped.Invoke((x) => x.Send(new UploadTeams.Request(response.Keys.ToImmutableArray()), ct))))
                        .ToTask(cancellationToken);
                    _logger.LogInformation("Finished Ingest {@Data}", response);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error ingesting file {ContainerName}{FilePath}", data.ContainerName, data.BlobPath);
                    if (data is {Response: { } r})
                    {
                        await new DiscordMessageBuilder()
                            .WithContent($"Error ingesting file {e.Message}")
                            .ModifyAsync(r);
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
