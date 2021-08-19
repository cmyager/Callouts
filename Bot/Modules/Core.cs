using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

// TODO: Probably delete this
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
            // This has to be here to let it register as a command
        }

        // TODO
        // Convert core.py
        //  - on_ready
        //  - clean_channel
        //  - before_clean_channel
        //  - on_member_remove (Might make sense to move this to user commands)
        //  - add_remove_offline_guilds
        //  - get_commands_channel
    }
}
