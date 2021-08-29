using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Callouts
{
    public class GuildManager
    {
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        private readonly DiscordClient Client;

        public GuildManager(IDbContextFactory<CalloutsContext> contextFactory, DiscordClient client)
        {
            ContextFactory = contextFactory;
            Client = client;
            Client.Ready += AddRemoveOfflineGuilds;
            Client.GuildCreated += RegisterNewGuild;
            Client.GuildDeleted += RemoveDeletedGuild;
        }
        public async Task<Guild> GetGuild(ulong guildId)
        {
            using var context = ContextFactory.CreateDbContext();

            IQueryable<Guild> guildQuery = context.Guilds.AsQueryable();
            guildQuery = guildQuery.Include(p => p.Events);
            guildQuery = guildQuery.Include(p => p.UserEvents);

            Guild guildConfig = await guildQuery.FirstOrDefaultAsync(p => p.GuildId == guildId,
                                     cancellationToken: CancellationToken.None);

            if (guildConfig == null)
            {
                guildConfig = new Guild() { GuildId = guildId };
                context.Add(guildConfig);
                await context.SaveChangesAsync();
            }
            return guildConfig;
        }

        public async Task<Guild> RemoveGuild(ulong guildId)
        {
            using var context = ContextFactory.CreateDbContext();
            var guildConfig = await GetGuild(guildId);
            if (guildConfig != null)
            {
                context.Remove(guildConfig);
                await context.SaveChangesAsync();
            }
            return guildConfig;
        }

        public async Task AddRemoveOfflineGuilds(DiscordClient sender, ReadyEventArgs e)
        {
            using var context = ContextFactory.CreateDbContext();
            var botGuilds = (from g in sender.Guilds select g.Value.Id);

            foreach (var guildId in botGuilds)
            {
                await GetGuild(guildId);
            }
            foreach (var guild in context.Guilds)
            {
                if (botGuilds.Contains(guild.GuildId) == false)
                {
                    await RemoveGuild(guild.GuildId);
                }
            }
        }

        private async Task RegisterNewGuild(DiscordClient sender, GuildCreateEventArgs e)
        {
            await GetGuild(e.Guild.Id);
        }

        private async Task RemoveDeletedGuild(DiscordClient sender, GuildDeleteEventArgs e)
        {
            await RemoveGuild(e.Guild.Id);
        }
    }
}
