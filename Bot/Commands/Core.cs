using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.IO;
using System.Text;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
using Callouts.DataContext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;

//using Callouts.Bot.Commands;

namespace Callouts
{
    public class Core : BaseCommandModule
    {
        private readonly DiscordClient client;
        private readonly GuildManager guildManager;

        public Core(DiscordClient client, GuildManager guildManager)
        {
            this.guildManager = guildManager;
            this.client = client;
            this.client.GuildCreated += Client_GuildCreated;
            this.client.GuildDeleted += Client_GuildDeleted;
        }
        private async Task<Guild> Client_GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            return await guildManager.GetGuild(e.Guild.Id);
        }
        private async Task<Guild> Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            return await guildManager.RemoveGuild(e.Guild.Id);
        }
    }
}
