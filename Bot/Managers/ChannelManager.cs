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
        //private readonly DiscordClient Client;

        private readonly List<string> RequiredChannels = new() { "bot-commands", "upcoming-events", "raid-reports" };

        /// <summary>
        /// ChannelManager
        /// </summary>
        /// <param name="client"></param>
        public ChannelManager(DiscordClient client)
        {
            //Client = client;
            client.GuildAvailable += CreateRequiredChannelsOnJoin;
            client.GuildCreated += CreateRequiredChannelsOnJoin;
        }

        /// <summary>
        /// GetChannel
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannel(DiscordGuild guild, string channelName)
        {
            var channel = (await guild.GetChannelsAsync()).FirstOrDefault(p => p.Name == channelName);
            if (channel == null)
            {
                List<DiscordOverwriteBuilder> overwriteList = new()
                {
                    new DiscordOverwriteBuilder(guild.EveryoneRole) { Allowed = Permissions.SendMessages | Permissions.AddReactions },
                    new DiscordOverwriteBuilder(guild.CurrentMember) { Allowed = Permissions.All }
                };
                channel = await guild.CreateTextChannelAsync(channelName, overwrites: overwriteList);
            }
            return channel;
        }

        /// <summary>
        /// CreateRequiredChannelsOnJoin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CreateRequiredChannelsOnJoin(DiscordClient sender, GuildCreateEventArgs e)
        {
            foreach (string channelName in RequiredChannels)
            {
                await GetChannel(e.Guild, channelName);
            }
        }
        // TODO
        //  - Clean channel function
        //  - Clean channel periodic task
    }
}
