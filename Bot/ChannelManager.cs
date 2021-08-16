using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

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
            var channel = guild.Channels.FirstOrDefault(p => p.Value.Name == channelName).Value;
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
