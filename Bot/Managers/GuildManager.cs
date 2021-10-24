using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Callouts
{
    public class GuildManager
    {
        private readonly string ChannelName = "bot-commands";
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        private readonly DiscordClient Client;
        private readonly ChannelManager channelManager;

        public GuildManager(IDbContextFactory<CalloutsContext> contextFactory, ChannelManager channelManager, DiscordClient client)
        {
            ContextFactory = contextFactory;
            Client = client;
            this.channelManager = channelManager;
            Client.Ready += OnReady;
            Client.Ready += AddRemoveOfflineGuilds;
            Client.GuildCreated += RegisterNewGuild;
            Client.GuildDeleted += RemoveDeletedGuild;
        }

        /// <summary>
        /// OnReady
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            channelManager.AddRequiredChannel(ChannelName);
            return Task.CompletedTask;
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


        // These are for the web. Could move them to a service or extention thing?
        public async Task<List<DiscordGuild>> GetGuildsFromUserId(ulong userId)
        {
            List<DiscordGuild> userGuilds = new();
            foreach ((ulong _, DiscordGuild guild) in Client.Guilds)
            {
                try
                {
                    if ((await guild.GetMemberAsync(userId)) != null)
                    {
                        userGuilds.Add(guild);
                    }
                }
                catch (Exception) { }

            }
            userGuilds = userGuilds.OrderBy(p => p.Name).ToList();
            return userGuilds;
        }

        public async Task<List<DiscordMember>> GetGuildMembersFromGuildId(ulong guildId)
        {
            List<DiscordMember> retval = new();

            DiscordGuild guild = await Client.GetGuildAsync(guildId);
            retval = (await guild.GetAllMembersAsync()).ToList();

            retval = retval.OrderBy(p => p.DisplayName).ToList();
            return retval;
        }

    }
}
