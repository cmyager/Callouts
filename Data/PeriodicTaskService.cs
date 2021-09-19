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
        // TODO: Reports on raid channel entry here or as a task?
        // TODO: Auto remove events older than 1 day
        private async void CleanChannelCallback(object? _)
        {
            EventManager eventManager = serviceProvider.GetRequiredService<EventManager>();
            DiscordClient client = serviceProvider.GetRequiredService<DiscordClient>();
            foreach (var guild in client.Guilds)
            {
                await eventManager.ListEvents(guild.Value);
            }
        }

        public PeriodicTaskService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public void StartPeriodicTimers()
        {
            this.CleanChannelsTimer = new Timer(CleanChannelCallback, null, TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
        }

        public void Dispose()
        {
            this.CleanChannelsTimer.Dispose();
        }
    }
}
