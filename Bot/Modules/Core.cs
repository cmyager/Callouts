using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace Callouts
{
    public class Core : BaseCommandModule
    {
        private readonly DiscordClient client;
        private readonly GuildManager guildManager;
        private readonly ChannelManager channelManager;

        public Core(DiscordClient client, GuildManager guildManager, ChannelManager channelManager)
        {
            this.guildManager = guildManager;
            this.client = client;
            this.channelManager = channelManager;
        }

        [Command("About"), Description("About")]
        public async Task About(CommandContext ctx)
        {
            // TODO
            // Uptime?
        }
    }
}
