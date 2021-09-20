using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Callouts.Data
{
    internal class PeriodicTaskService : IDisposable
    {
        public IServiceProvider serviceProvider;
        private Timer CleanChannelsTimer { get; set; }
        // TODO: Could have one to update the bungie.net manifest

        private async void CleanChannelCallback(object? _)
        {
            GuildManager guildManager = serviceProvider.GetRequiredService<GuildManager>();
            EventManager eventManager = serviceProvider.GetRequiredService<EventManager>();
            ReportManager reportManager = serviceProvider.GetRequiredService<ReportManager>();
            DiscordClient client = serviceProvider.GetRequiredService<DiscordClient>();
            foreach (var guild in client.Guilds)
            {
                // Make sure the guild exists
                await guildManager.GetGuild(guild.Value.Id);

                // Clean Events Channel
                await eventManager.ListEvents(guild.Value);

                // Clean reports Channel
                await reportManager.CleanChannel(guild.Value);

                // TODO: Bot commands channel
            }
        }

        public PeriodicTaskService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public void StartPeriodicTimers()
        {
            this.CleanChannelsTimer = new Timer(CleanChannelCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromHours(1));
        }

        public void Dispose()
        {
            this.CleanChannelsTimer.Dispose();
        }
    }
}
