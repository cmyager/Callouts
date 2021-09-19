using BungieSharper.Entities;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Linq;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace Callouts
{
    public class UserManager
    {
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        //private readonly DiscordClient Client;
        public UserManager(IDbContextFactory<CalloutsContext> contextFactory, DiscordClient client)
        {
            ContextFactory = contextFactory;
            //Client = client;
            //client.Ready += RemoveOfflineUsers;
            client.GuildMemberRemoved += RemoveGuildMemberRemoved;
            //client.GuildMemberAdded += AddGuildMember;
        }

        // This is a possible todo.
        // TODO: If a user is in an event and they are no longer in the server the bot crashes
        //public async Task RemoveOfflineUsers(DiscordClient sender, ReadyEventArgs e)
        //{
        //}

        // This is mainly for testing. I don't think I want to register everyone the moment they join the server
        //public async Task AddGuildMember(DiscordClient sender, GuildMemberAddEventArgs e)
        //{
        //    await GetUserByUserId(e.Member.Id);
        //}

        public async Task RemoveGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            bool found = false;
            foreach (var guild in sender.Guilds)
            {
                if (found)
                {
                    break;
                }
                foreach (var member in (await guild.Value.GetAllMembersAsync()))
                {
                    if (member.Id == e.Member.Id)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                _ = RemoveUser(e.Member.Id);
            }
        }

        public async Task<User> GetUserByUserId(ulong UserId)
        {
            using var context = ContextFactory.CreateDbContext();
            IQueryable<User> UserQuery = context.Users.AsQueryable();
            UserQuery = UserQuery.Include(p => p.Events);
            UserQuery = UserQuery.Include(p => p.UserEvents);
            var user = await UserQuery.FirstOrDefaultAsync(p => p.UserId == UserId,
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

        public async Task<User> GetUserByPlatformId(BungieMembershipType platform, long platformId)
        {
            using var context = ContextFactory.CreateDbContext();
            return await context.Users.AsQueryable()
                .FirstOrDefaultAsync(p => p.PrimaryPlatformId == platformId && p.Platform == platform,
                                     cancellationToken: CancellationToken.None);
        }

        public async Task UpdatePlatform(ulong UserId, BungieMembershipType platform)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            user.Platform = platform;
            context.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateRegistration(ulong UserId, long BungieId)
        {
            using var context = ContextFactory.CreateDbContext();
            var user = await GetUserByUserId(UserId);
            user.BungieId = BungieId;
            context.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task<User> SyncBungieProfile(ulong UserId, UserMembershipData bungieMembership, DestinyProfileResponse bungieProfile)
        {
            using var context = ContextFactory.CreateDbContext();
            var discordUser = await GetUserByUserId(UserId);
            discordUser.BungieId = bungieMembership.BungieNetUser.MembershipId;
            discordUser.BungieName = bungieMembership.BungieNetUser.DisplayName;

            foreach (var membership in bungieMembership.DestinyMemberships)
            {
                if (membership.MembershipId == bungieMembership.PrimaryMembershipId)
                {
                    discordUser.Platform = membership.MembershipType;
                    discordUser.PrimaryPlatformId = membership.MembershipId;
                    discordUser.PrimaryPlatformName = membership.DisplayName;
                }
            }
            context.Update(discordUser);
            await context.SaveChangesAsync();
            return discordUser;
        }

        public async Task<User> ClearBungieProfile(ulong UserId)
        {
            using var context = ContextFactory.CreateDbContext();
            var discordUser = await GetUserByUserId(UserId);
            discordUser.UnlinkBungieAccount();
            context.Update(discordUser);
            await context.SaveChangesAsync();
            return discordUser;
        }
    }
}