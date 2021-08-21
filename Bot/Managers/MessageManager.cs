using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Callouts.DataContext;
using Callouts.Data;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
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
using BungieSharper.Entities.Destiny.HistoricalStats.Definitions;
using DSharpPlus.Entities;
using BungieSharper.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Callouts
{
    public class MessageManager : IDisposable
    {
        private readonly CommandContext Ctx;
        private List<DiscordMessage> MessagesToClean = new();
        public ulong UserId;
        //private bool disposed = false;
        public MessageManager(CommandContext ctx)
        {
            Ctx = ctx;
            if (Ctx.Channel.IsPrivate)
            {
                UserId = Ctx.Message.Author.Id;
            }
            else
            {
                UserId = Ctx.Member.Id;
            }
            Ctx.TriggerTypingAsync();
            AddMessageToClean(Ctx.Message);
        }

        public void Dispose()
        {
            CleanMessages();
            GC.SuppressFinalize(this);
        }

        public void AddMessageToClean(DiscordMessage message)
        {
            if (!Ctx.Channel.IsPrivate && Ctx.Message != null)
            {
                MessagesToClean.Add(Ctx.Message);
            }
        }

        public void CleanMessages()
        {
            if (MessagesToClean.Count > 0)
            {
                _ = Ctx.Channel.DeleteMessagesAsync(MessagesToClean);
            }
        }

        public async Task<DiscordMessage> SendEmbed(DiscordEmbed embed)
        {
            return await Ctx.Channel.SendMessageAsync(embed);
        }

        // TODO: If needed convert the other methods
        // - get_next_message
        // - get_next_private_message
        // - send_private_message
        // - send_private_embed
        //}
        //public async Task<DiscordMessage> SendMessage(string messageText)
        //{
        //    if (Ctx.Channel.IsPrivate)
        //    {
        //        Ctx.Message.Author
        //        Ctx.Guild.Members.GetValueOrDefault()
        //        DiscordMessage message = 
        //    }
        //}
    }
}
