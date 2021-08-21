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
using BungieSharper.Entities;
using System.ComponentModel.DataAnnotations;


// TODO: Other stats like
// - Rumble
// - Doubles
// - Scorched
// - Mayhem

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

        private Dictionary<BungieMembershipType, string> PlatformIconUrls = new Dictionary<BungieMembershipType, string>
        {
            { BungieMembershipType.TigerXbox, "https://www.bungie.net/img/theme/bungienet/icons/xboxLiveLogo.png" },
            { BungieMembershipType.TigerPsn, "https://www.bungie.net/img/theme/bungienet/icons/psnLogo.png" },
            { BungieMembershipType.TigerSteam, "https://www.bungie.net/img/theme/bungienet/icons/steamLogo.png" },
            { BungieMembershipType.TigerStadia, "https://www.bungie.net/img/theme/bungienet/icons/stadiaLogo.png" }
        };

        public Stats(DiscordClient client, GuildManager guildManager, ChannelManager channelManager, BungieService bungieService, UserManager userManager)
        {
            this.guildManager = guildManager;
            this.client = client;
            this.channelManager = channelManager;
            this.bungieService = bungieService;
            this.userManager = userManager;
        }


        [Command("stats"), Description("BaseStats")]
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
            using var messageManager = new MessageManager(ctx);
            var discordUser = await userManager.GetUserByUserId(messageManager.UserId);

            var activityMode = new List<DestinyActivityModeType>
            {
                DestinyActivityModeType.AllPvE,
                DestinyActivityModeType.Raid,
                DestinyActivityModeType.Nightfall,
                DestinyActivityModeType.AllStrikes,
                DestinyActivityModeType.ScoredNightfall,
                DestinyActivityModeType.ScoredHeroicNightfall,
                DestinyActivityModeType.HeroicNightfall
            };
            var historicalStats = await bungieService.GetHistoricalStats(discordUser.PrimaryPlatformId.Value, discordUser.Platform, activityMode);
            BungieService.PvEStats pveStats = new BungieService.PvEStats(historicalStats);
            await messageManager.SendEmbed(GeneratePveStatsEmbed(pveStats, discordUser));
        }


        [Command("pvp"), Description("Display PvP stats across all characters on your account")]
        public async Task StatsPvp(CommandContext ctx)
        {
            using var messageManager = new MessageManager(ctx);
            var discordUser = await userManager.GetUserByUserId(messageManager.UserId);
            var activityMode = new List<DestinyActivityModeType> { DestinyActivityModeType.AllPvP };
            var historicalStats = await bungieService.GetHistoricalStats(discordUser.PrimaryPlatformId.Value, discordUser.Platform, activityMode);           
            BungieService.PvPStats pvpStats = new BungieService.PvPStats(historicalStats["allPvP"].AllTime);
            await messageManager.SendEmbed(GeneratePvPStatsEmbed(pvpStats, discordUser, "Crucible Stats"));
        }

        [Command("trials"), Description("Display Trials stats across all characters on your account")]
        public async Task StatsTrials(CommandContext ctx)
        {
            using var messageManager = new MessageManager(ctx);
            var discordUser = await userManager.GetUserByUserId(messageManager.UserId);
            var activityMode = new List<DestinyActivityModeType> { DestinyActivityModeType.TrialsOfOsiris };
            var historicalStats = await bungieService.GetHistoricalStats(discordUser.PrimaryPlatformId.Value, discordUser.Platform, activityMode);
            BungieService.PvPStats pvpStats = new BungieService.PvPStats(historicalStats["trials_of_osiris"].AllTime);
            await messageManager.SendEmbed(GeneratePvPStatsEmbed(pvpStats, discordUser, "Trials of Osiris Stats"));
        }

        [Command("ib"), Description("Display Iron Banner stats across all characters on your account")]
        public async Task StatsIb(CommandContext ctx)
        {
            using var messageManager = new MessageManager(ctx);
            var discordUser = await userManager.GetUserByUserId(messageManager.UserId);
            var activityMode = new List<DestinyActivityModeType> { DestinyActivityModeType.IronBanner };
            var historicalStats = await bungieService.GetHistoricalStats(discordUser.PrimaryPlatformId.Value, discordUser.Platform, activityMode);
            BungieService.PvPStats pvpStats = new BungieService.PvPStats(historicalStats["ironBanner"].AllTime);
            await messageManager.SendEmbed(GeneratePvPStatsEmbed(pvpStats, discordUser, "Iron Banner Stats"));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // TODO: There is probably a better way to deal with embeds in general that isn't so hard coded
        /////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Created discord embed from PvPStats
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="discordUser"></param>
        /// <param name="statsType"></param>
        /// <returns> Discord embed containing PVP Stats </returns>
        private DiscordEmbedBuilder GeneratePvPStatsEmbed(BungieService.PvPStats stats, User discordUser, string statsType)
        {
            var e = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{discordUser.PrimaryPlatformName} | {statsType}",
                    IconUrl = PlatformIconUrls[discordUser.Platform]
                }
            };
            e.AddField("Kills", stats.Kills, true);
            e.AddField("Assists", stats.Assists, true);
            e.AddField("Deaths", stats.Deaths, true);
            e.AddField("KDR", stats.Kdr, true);
            e.AddField("KDA", stats.Kda, true);
            e.AddField("Best Single Game Kills", stats.BestSingleGameKills, true);
            e.AddField("Games Played", stats.GamesPlayed, true);
            e.AddField("Games Won", stats.ActivitiesWon, true);
            e.AddField("Win Rate", stats.WinRate, true);
            e.AddField("Combat Raiting", stats.CombatRating, true);
            e.AddField("Favorite Weapon", stats.BestWeapon, true);
            e.AddField("Average Lifespan", stats.AverageLifeSpan, true);
            e.AddField("Longest Spree", stats.LongestSpree, true);
            e.AddField("Longest Life", stats.LongestLife, true);
            e.AddField("Longest Kill Distance", $"{stats.LongestKillDistance} M", true);
            e.AddField("Time Played", stats.TimePlayed, true);

            return e;
        }

        /// <summary>
        /// Created discord embed from PvEStats
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="discordUser"></param>
        /// <returns> Discord embed containing PVE Stats </returns>
        private DiscordEmbedBuilder GeneratePveStatsEmbed(BungieService.PvEStats stats, User discordUser)
        {
            var e = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{discordUser.PrimaryPlatformName} | PvE Stats",
                    IconUrl = PlatformIconUrls[discordUser.Platform]
                }
            };
            e.AddField("Kills", stats.Kills, true);
            e.AddField("Assists", stats.Assists, true);
            e.AddField("Deaths", stats.Deaths, true);
            e.AddField("Best Single Game Kills", stats.BestSingleGameKills, true);
            e.AddField("Total Opponents Defeated", stats.OpponentsDefeated, true);
            e.AddField("Favorite Weapon", stats.BestWeapon, true);
            e.AddField("Average Lifespan", stats.AverageLifeSpan, true);
            e.AddField("Longest Spree", stats.LongestSpree, true);
            e.AddField("Longest Life", stats.LongestLife, true);
            e.AddField("Longest Kill Distance", $"{stats.LongestKillDistance} M", true);
            e.AddField("Time Played", stats.TimePlayed, true);
            e.AddField("Strikes", stats.StrikeCount, true);
            e.AddField("Nightfalls", stats.NightFallCount, true);
            e.AddField("Fastest Nightfall", stats.FastestNightfall, true);
            e.AddField("Public Events", stats.EventCount, true);
            e.AddField("Heroic Public Events", stats.HeroicEventCount, true);
            e.AddField("Total Raids", stats.RaidCount, true);
            e.AddField("Total Raid Time", stats.RaidTime, true);

            return e;
        }
    }
}
