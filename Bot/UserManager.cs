using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Callouts.DataContext;

namespace Callouts
{
    public class UserManager
    {
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        public UserManager(IDbContextFactory<CalloutsContext> contextFactory)
        {
            ContextFactory = contextFactory;
        }

        public async Task<User> GetUserByUserId(ulong UserId)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await context.Users.AsQueryable()
                .FirstOrDefaultAsync(p => p.UserId == UserId,
                                     cancellationToken: CancellationToken.None);
            if (user == null)
            {
                user = new User() { UserId = UserId };
                context.Add(user);
                await context.SaveChangesAsync();
            }
            return user;
        }

        public async Task<User> RemoveUser(ulong UserId)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            if (user != null)
            {
                context.Remove(user);
                await context.SaveChangesAsync();
            }
            return user;
        }

        public async Task<User> GetUserByPlatformId(Platform platform, ulong UserId)
        {
            // TODO: make this work for all the platforms
            using var context = ContextFactory.CreateDbContext();
            return await context.Users.AsQueryable()
                .FirstOrDefaultAsync(p => p.UserId == UserId,
                                     cancellationToken: CancellationToken.None);
        }
        public async Task UpdateDisplayNames(ulong UserId, string XboxName, string PsnName, string SteamName, string StadiaName)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            user.XboxName = XboxName;
            user.PsnName = PsnName;
            user.SteamName = SteamName;
            user.StadiaName = StadiaName;
            context.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateMembershipIds(ulong UserId, ulong XboxId, ulong PsnId, ulong SteamId, ulong StadiaId)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            user.XboxId = XboxId;
            user.PsnId = PsnId;
            user.SteamId = SteamId;
            user.StadiaId = StadiaId;
            context.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePlatform(ulong UserId, Platform platform)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            user.Platform = platform;
            context.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateRegistration(ulong UserId, ulong BungieId)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            user.BungieId = BungieId;
            context.Update(user);
            await context.SaveChangesAsync();
        }

    }
}