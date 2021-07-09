using System;
using Azure.Storage.Blobs;
using DryIoc;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.DryIoc;
using Rocket.Surgery.Conventions.Reflection;
using Serilog.Events;
using Serilog.Extensions.Logging;
using wotbot.Workers;

namespace wotbot.Infrastructure
{
    [ExportConvention]
    class DiscordConvention : IDryIocConvention
    {
        public IContainer Register(IConventionContext conventionContext, IConfiguration configuration, IServiceCollection services, IContainer container)
        {
            services.AddHttpClient();
            ConfigureDiscordClients(conventionContext, container);
            container.RegisterMany<BlobContainerClientClientFactory>(Reuse.Singleton);
            container.RegisterMany<TableClientFactory>(Reuse.Singleton);
            AddHostedServices(services);
            ConfigureOptions(configuration, services);

            return container;
        }

        private static void ConfigureDiscordClients(IConventionContext conventionContext, IContainer container)
        {
            container.RegisterDelegate<IOptions<DiscordConfiguration>, IOptions<CommandsNextConfiguration>, DiscordClient>(
                reuse: Reuse.Singleton,
                factory: (discordOptions, commandsNextOptions) =>
                {
                    var discordClient = new DiscordClient(discordOptions.Value);
                    var commands = discordClient.UseCommandsNext(commandsNextOptions.Value);
                    foreach (var assembly in conventionContext.AssemblyCandidateFinder.GetCandidateAssemblies("DSharpPlus.CommandsNext"))
                        commands.RegisterCommands(assembly);
                    return discordClient;
                });
            container.RegisterDelegate<IOptions<DiscordConfiguration>, DiscordRestClient>(
                reuse: Reuse.Singleton,
                factory: (discordOptions) => new DiscordRestClient(discordOptions.Value));
        }

        private static void AddHostedServices(IServiceCollection services)
        {
            services.AddHostedService<DiscordBotService>();
            services.AddHostedService<ProfessionsWorker>();
            services.AddHostedService<SavedVariablesWorker>();
        }

        private static void ConfigureOptions(IConfiguration configuration, IServiceCollection services)
        {
            services.AddOptions<DiscordOptions>()
                .BindConfiguration("Discord");

            services.AddOptions<CommandsNextConfiguration>()
                .Configure<IServiceProvider>((nextConfiguration, serviceProvider) =>
                {
                    nextConfiguration.Services = serviceProvider;
                    nextConfiguration.EnableMentionPrefix = true;
                    nextConfiguration.StringPrefixes = new[] {"!"};
                    // get assemblies here
                    // configuration.Services =
                });

            services.AddOptions<DiscordConfiguration>()
                .Configure<IOptions<DiscordOptions>>((discordConfiguration, options) =>
                {
                    discordConfiguration.TokenType = TokenType.Bot;
                    discordConfiguration.Intents = DiscordIntents.AllUnprivileged
                        ;
                    discordConfiguration.Token = options.Value.Token;
                })
                .Configure<ILoggerFactory>((discordConfiguration, loggerFactory) =>
                {
                    discordConfiguration.LoggerFactory = loggerFactory;
                    discordConfiguration.MinimumLogLevel = LevelConvert.ToExtensionsLevel(configuration.GetValue<LogEventLevel>("Serilog:Default:MinimumLevel"));
                });
        }
    }
}
