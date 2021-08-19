using Callouts.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Callouts
{
    public class GuildManager
    {
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;

        public GuildManager(IDbContextFactory<CalloutsContext> contextFactory)
        {
            ContextFactory = contextFactory;
        }
        public async Task<Guild> GetGuild(ulong guildId)
        {
            using var context = ContextFactory.CreateDbContext();
            var guildConfig = await context.Guilds.AsQueryable()
                .FirstOrDefaultAsync(p => p.GuildId == guildId,
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
    }
}
