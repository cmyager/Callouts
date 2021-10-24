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
        private static readonly string WelcomeChannelName = "welcome";
        // private readonly RoleManager RoleManager;
        private readonly List<string> RequiredChannels = new() { WelcomeChannelName };

        /// <summary>
        /// ChannelManager
        /// </summary>
        /// <param name="client"></param>
        // public ChannelManager(DiscordClient client, RoleManager roleManager)
        public ChannelManager(DiscordClient client)
        {
            //Client = client;
            // this.RoleManager = roleManager;
            // client.GuildAvailable += CreateRequiredChannelsOnJoin;
            // client.GuildCreated += CreateRequiredChannelsOnJoin;
        }

        public void AddRequiredChannel(string channelName)
        {
            if (!RequiredChannels.Contains(channelName))
            {
                RequiredChannels.Add(channelName);
            }
        }

        /// <summary>
        /// GetChannel
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannel(DiscordGuild guild, string channelName, ChannelType type=ChannelType.Text)
        {
            var channel = (await guild.GetChannelsAsync()).FirstOrDefault(p => p.Name == channelName && p.Type == type);
            if (channel == null)
            {
                List<DiscordOverwriteBuilder> overwriteList = null;
                if (channelName == WelcomeChannelName)
                {
                    overwriteList = new()
                    {
                        new DiscordOverwriteBuilder(guild.EveryoneRole) { Allowed = Permissions.AccessChannels | Permissions.ReadMessageHistory }
                    };
                }
                channel = await guild.CreateChannelAsync(channelName, type, overwrites: overwriteList);
            }
            return channel;
        }

        /// <summary>
        /// CreateRequiredChannelsOnJoin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        // private async Task CreateRequiredChannelsOnJoin(DiscordClient sender, GuildCreateEventArgs e)
        // {
        //     foreach (string channelName in RequiredChannels)
        //     {
        //         DiscordChannel channel = await GetChannel(e.Guild, channelName);
        //         if (channelName == WelcomeChannelName)
        //         {
        //             // await RoleManager.PostWelcomeMessage(channel);
        //         }
        //     }
        // }
    }
}
