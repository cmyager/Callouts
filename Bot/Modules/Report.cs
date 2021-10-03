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
using Discord.OAuth2;
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
using System.Linq;
using DSharpPlus.Entities;
using BungieSharper.Entities;
using System.ComponentModel.DataAnnotations;
using Callouts.Attributes;

namespace Callouts
{
    [Description("Posts your most recent raid report."), RequireBungieLink()]
    public class Report : BaseCommandModule
    {
        private readonly DiscordClient client;
        private readonly GuildManager guildManager;
        private readonly ChannelManager channelManager;
        private readonly ReportManager reportManager;
        private readonly BungieService bungieService;
        private readonly UserManager userManager;

        public Report(DiscordClient client,
                      GuildManager guildManager,
                      ChannelManager channelManager,
                      ReportManager reportManager,
                      BungieService bungieService,
                      UserManager userManager)
        {
            this.client = client;
            this.guildManager = guildManager;
            this.channelManager = channelManager;
            this.reportManager = reportManager;
            this.bungieService = bungieService;
            this.userManager = userManager;
        }

        [Command("report"), Description("Posts your most recent raid report.")]
        public Task RaidReport(CommandContext ctx)
        {
            using var messageManager = new MessageManager(ctx);
            if (messageManager.IsPrivate)
            {
                _ = messageManager.SendMessage("Sorry this doesn't work in private messages yet.");
            }
            else
            {
                _ = reportManager.RequestReport(messageManager.UserId, messageManager.GuildId);
            }
            return Task.CompletedTask;
        }
    }
}
