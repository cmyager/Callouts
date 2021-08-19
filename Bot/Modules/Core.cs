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
        public readonly string channelName = "bot-commands";

        public Core(DiscordClient client, GuildManager guildManager, ChannelManager channelManager)
        {
            this.guildManager = guildManager;
            this.client = client;
            this.channelManager = channelManager;
            this.client.GuildCreated += Client_GuildCreated;
            this.client.GuildDeleted += Client_GuildDeleted;
            this.client.Ready += Core_Ready;
        }
        private async Task<Guild> Client_GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            await channelManager.GetChannel(e.Guild, channelName);
            return await guildManager.GetGuild(e.Guild.Id);
        }
        private async Task<Guild> Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            return await guildManager.RemoveGuild(e.Guild.Id);
            // Call managers and have them purge old things
        }
        private async Task Core_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            // TODO: Add remove offline guilds
            foreach (var guild in sender.Guilds)
            {
                var channelthing = await channelManager.GetChannel(guild.Value, channelName);
            }
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
