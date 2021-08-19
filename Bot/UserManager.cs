using BungieSharper.Entities;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Callouts.DataContext;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<User> GetUserByPlatformId(BungieMembershipType platform, ulong UserId)
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

        public async Task UpdateMembershipIds(ulong UserId, long XboxId, long PsnId, long SteamId, long StadiaId)
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
            discordUser.PsnName = bungieMembership.BungieNetUser.PsnDisplayName;
            discordUser.StadiaName = bungieMembership.BungieNetUser.StadiaDisplayName;
            discordUser.SteamName = bungieMembership.BungieNetUser.SteamDisplayName;
            discordUser.XboxName = bungieMembership.BungieNetUser.XboxDisplayName;
            // Could use a case statement here?
            foreach (var membership in bungieMembership.DestinyMemberships)
            {
                if (membership.MembershipType == BungieMembershipType.TigerPsn)
                {
                    discordUser.PsnId = membership.MembershipId;
                }
                else if (membership.MembershipType == BungieMembershipType.TigerStadia)
                {
                    discordUser.StadiaId = membership.MembershipId;
                }
                else if (membership.MembershipType == BungieMembershipType.TigerSteam)
                {
                    discordUser.SteamId = membership.MembershipId;
                }
                else if (membership.MembershipType == BungieMembershipType.TigerXbox)
                {
                    discordUser.XboxId = membership.MembershipId;
                }

                if (membership.MembershipId == bungieMembership.PrimaryMembershipId)
                {
                    discordUser.Platform = membership.MembershipType;
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