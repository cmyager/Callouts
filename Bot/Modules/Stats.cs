using Callouts.DataContext;
using Callouts.Data;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using BungieSharper.Client;
using Callouts.Data;
using Callouts.DataContext;
using Discord.OAuth2;
using DSharpPlus;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plk.Blazor.DragDrop;
using System;
using System.Collections.Generic;
using BungieSharper.Entities.Destiny.HistoricalStats.Definitions;
using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Callouts
{
    [Group("stats"), Description("Display various Destiny 2 stats")]
    public class Stats : BaseCommandModule
    {
        private readonly DiscordClient client;
        private readonly GuildManager guildManager;
        private readonly ChannelManager channelManager;
        private readonly BungieService bungieService;
        private readonly UserManager userManager;

        public Stats(DiscordClient client, GuildManager guildManager, ChannelManager channelManager, BungieService bungieService, UserManager userManager)
        {
            this.guildManager = guildManager;
            this.client = client;
            this.channelManager = channelManager;
            this.bungieService = bungieService;
            this.userManager = userManager;
        }


        [Command("stats"),  Description("BaseStats")]
        public async Task BaseStats(CommandContext ctx)
        {
            await ctx.RespondAsync("Not specified");
            // TODO
            // OLD HELP
            //Display various Destiny 2 stats
            //!stats pve
            //Display PvE stats across all characters on your account
            //!stats trials
            //Display Trials stats across all characters on your account
            //!stats ib
            //Display Iron Banner stats across all characters on your account
            //!stats pvp
            //Display PvP stats across all characters on your account
            //Use!help[command] for more info on a command

        }
        
        [Command("pve"), Description("Display PvE stats across all characters on your account")]
        public async Task StatsPve(CommandContext ctx)
        {
            var discordUser = await userManager.GetUserByUserId(ctx.Member.Id);
            
            var activityMode = new List<DestinyActivityModeType>
            {
                DestinyActivityModeType.AllPvE,
                DestinyActivityModeType.Raid,
                DestinyActivityModeType.Nightfall,
                DestinyActivityModeType.AllStrikes,
                DestinyActivityModeType.ScoredNightfall,
                DestinyActivityModeType.ScoredHeroicNightfall
            };
            var x = await bungieService.GetHistoricalStats(discordUser.PrimaryPlatformId.Value, discordUser.Platform, activityMode);
            BungieService.PvEStats pveStats = new BungieService.PvEStats(x);
            
            await ctx.RespondAsync("pve");
        }


        [Command("pvp"), Description("Display PvP stats across all characters on your account")]
        public async Task StatsPvp(CommandContext ctx)
        {
            await ctx.RespondAsync("pvp");
        }

        [Command("trials"), Description("Display Trials stats across all characters on your account")]
        public async Task StatsTrials(CommandContext ctx)
        {
            await ctx.RespondAsync("trials");
        }

        [Command("ib"), Description("Display Iron Banner stats across all characters on your account")]
        public async Task StatsIb(CommandContext ctx)
        {
            await ctx.RespondAsync("ib");
        }


    }
}
