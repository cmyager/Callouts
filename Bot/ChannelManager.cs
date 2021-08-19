using DSharpPlus;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Callouts
{
    public class ChannelManager
    {
        private readonly DiscordClient Client;
        public ChannelManager(DiscordClient client)
        {
            Client = client;
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
        // TODO
        //  - Support overwrites and other settings in the channel creation.
        //  - Probably makes sense to break get to get/create
        //  - Clean channel function
        //  - Clean channel periodic task
    }
}
