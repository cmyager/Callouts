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
        private Timer ClearGuestRolesTimer { get; set; }

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

                // Bot commands channel
            }
        }
        // private async void ClearGuestRolesCallback(object? _)
        // {
        //     RoleManager roleManager = serviceProvider.GetRequiredService<RoleManager>();
        //     await roleManager.ClearGuestRolls();
        // }

        public PeriodicTaskService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public void StartPeriodicTimers()
        {
            CleanChannelsTimer = new Timer(CleanChannelCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromHours(1));
            // ClearGuestRolesTimer = new Timer(ClearGuestRolesCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromDays(1));
        }

        public void Dispose()
        {
            this.CleanChannelsTimer.Dispose();
        }
    }
}
