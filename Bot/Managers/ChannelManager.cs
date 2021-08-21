using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callouts
{
    public class ChannelManager
    {
        private readonly DiscordClient Client;

        private readonly List<string> RequiredChannels = new() { "bot-commands", "upcoming-events", "raid-reports" };

        public ChannelManager(DiscordClient client)
        {
            Client = client;
            Client.Ready += VerifyRequiredChannelsOnStartup;
            Client.GuildCreated += CreateRequiredChannelsOnJoin;
        }

        public async Task<DiscordChannel> GetChannel(DiscordGuild guild, string channelName)
        {
            var channel = (await guild.GetChannelsAsync()).FirstOrDefault(p => p.Name == channelName);
            if (channel == null)
            {
                channel = await guild.CreateChannelAsync(channelName, ChannelType.Text);
            }
            return channel;
        }

        private async Task VerifyRequiredChannelsOnStartup(DiscordClient sender, ReadyEventArgs e)
        {
            foreach (var guild in sender.Guilds)
            {
                foreach (string channelName in RequiredChannels)
                {
                    await GetChannel(guild.Value, channelName);
                }
            }
        }

        private async Task CreateRequiredChannelsOnJoin(DiscordClient sender, GuildCreateEventArgs e)
        {
            foreach (string channelName in RequiredChannels)
            {
                await GetChannel(e.Guild, channelName);
            }
        }

        // TODO
        //  - Support overwrites and other settings in the channel creation. Here is itin python
        //overwrites = {guild.default_role: discord.PermissionOverwrite(send_messages=False, add_reactions=True),
        //              guild.me: discord.PermissionOverwrite(send_messages=True, add_reactions=True)}
    //  - Clean channel function
    //  - Clean channel periodic task
}
}
