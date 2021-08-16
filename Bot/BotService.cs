using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
using Callouts.DataContext;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Callouts
{
    internal class BotService : IHostedService
    {

        public readonly EventId BotEventId = new EventId(42, "BotService");
        public DiscordClient Client { get; set; }
        public InteractivityExtension Interactivity { get; set; }
        public CommandsNextExtension Commands { get; set; }

        private readonly GuildManager guildManager;
        private readonly UserManager userManager;
        private readonly ChannelManager channelManager;
        private readonly IConfiguration config;
        private readonly ILogger<BotService> logger;
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;

        public BotService(DiscordClient client,
                          GuildManager guildManager,
                          UserManager userManager,
                          ChannelManager channelManager,
                          IConfiguration config,
                          ILogger<BotService> logger,
                          IDbContextFactory<CalloutsContext> ContextFactory)
        {
            this.Client = client;
            this.guildManager = guildManager;
            this.userManager = userManager;
            this.channelManager = channelManager;
            this.config = config;
            this.logger = logger;
            this.ContextFactory = ContextFactory;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;

            // Enable interactivity, and set default options
            Client.UseInteractivity(new InteractivityConfiguration
            {
                // default pagination behaviour to just ignore the reactions
                PaginationBehaviour = PaginationBehaviour.Ignore,
                // default timeout for other actions to 2 minutes
                Timeout = TimeSpan.FromMinutes(2)
            });

            var services = new ServiceCollection();
            services.AddSingleton<DiscordClient>(Client);
            services.AddSingleton<GuildManager>(guildManager);
            services.AddSingleton<UserManager>(userManager);
            services.AddSingleton<ChannelManager>(channelManager);

            // Set up commands
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { config["Discord:prefix"] },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = services.BuildServiceProvider()
            });

            // let's hook some command events, so we know what's going on
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            // Register commands
            Commands.RegisterCommands<ExampleInteractiveCommands>();
            Commands.RegisterCommands<Core>();

            // Connect and log in
            await Client.ConnectAsync();
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
            return Task.CompletedTask;
        }
        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }
        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");
            return Task.CompletedTask;
        }
        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }
        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    //red
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync(embed);
            }
        }
    }
}