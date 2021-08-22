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

        public async Task<DiscordMessage> SendMessage(string content)
        {
            return await SendMessage(new DiscordMessageBuilder() { Content = content });
        }
        public async Task<DiscordMessage> SendMessage(DiscordEmbed embed)
        {
            return await SendMessage(new DiscordMessageBuilder() { Embed = embed });
        }
        public async Task<DiscordMessage> SendMessage(string content, DiscordEmbed embed)
        {
            return await SendMessage(new DiscordMessageBuilder() { Content = content, Embed = embed });
        }
        public async Task<DiscordMessage> SendMessage(DiscordMessageBuilder builder)
        {
            return await Ctx.Channel.SendMessageAsync(builder);
        }


        public async Task<DiscordMessage> SendPrivateMessage(string content)
        {
            return await SendPrivateMessage(new DiscordMessageBuilder() { Content = content });
        }
        public async Task<DiscordMessage> SendPrivateMessage(DiscordEmbed embed)
        {
            return await SendPrivateMessage(new DiscordMessageBuilder() { Embed = embed });
        }
        public async Task<DiscordMessage> SendPrivateMessage(string content, DiscordEmbed embed)
        {
            return await SendPrivateMessage(new DiscordMessageBuilder() { Content = content, Embed = embed });
        }
        public async Task<DiscordMessage> SendPrivateMessage(DiscordMessageBuilder builder)
        {
            if (Ctx.Channel.IsPrivate)
            {
                return await SendMessage(builder);
            }
            else
            {
                return await Ctx.Member.SendMessageAsync(builder);
            }
        }






        //public async Task<DiscordMessage> SendRegistrationMessage()
        //{
        //    //if (Ctx.Guild.GetMemberAsync()
        //}

        // TODO: If needed convert the other methods
        // - get_next_message
        // - get_next_private_message
    }
}
