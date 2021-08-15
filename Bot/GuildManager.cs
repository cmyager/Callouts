using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Callouts.DataContext;

namespace Callouts
{
    public class GuildManager
    {
        private readonly IDbContextFactory<CalloutsContext> mContextFactory;
        public GuildManager(IDbContextFactory<CalloutsContext> contextFactory)
        {
            mContextFactory = contextFactory;
        }
        public async Task<Guild> GetGuild(ulong guildId)
        {
            using var context = mContextFactory.CreateDbContext();
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
            using var context = mContextFactory.CreateDbContext();
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
