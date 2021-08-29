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

namespace Callouts.Data
{
    public enum ScheduledTaskType : byte
    {
        Unknown = 0,
        SendReminder = 1,
        GetReport = 2,
        CleanChannel = 3
    }

    public abstract class ScheduledTask : IEquatable<ScheduledTask>
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public DateTimeOffset ExecutionTime { get; set; }
        public bool IsExecutionTimeReached => this.TimeUntilExecution.CompareTo(TimeSpan.Zero) < 0;
        public abstract TimeSpan TimeUntilExecution { get; }
        public bool Equals(ScheduledTask? other) => other is { } && this.Id == other.Id;
        public override bool Equals(object? obj) => this.Equals(obj as ScheduledTask);
        public override int GetHashCode() => this.Id.GetHashCode();

    }
    public class Reminder : ScheduledTask, IEquatable<Reminder>
    {
        //public ulong ChannelId { get => (ulong)this.ChannelIdDb.GetValueOrDefault(); set => this.ChannelIdDb = (long)value; }
        //public string Message { get; set; } = null!;
        public bool IsRepeating { get; set; } = false;
        public TimeSpan? RepeatIntervalDb { get; set; }
        public TimeSpan RepeatInterval => this.RepeatIntervalDb ?? TimeSpan.FromMilliseconds(-1);
        public DiscordEmbedBuilder EventEmbed { get; set; }
        public DiscordMember Member { get; set; } = null;
        public override TimeSpan TimeUntilExecution
        {
            get
            {
                DateTimeOffset now = DateTimeOffset.Now;
                if (this.ExecutionTime > now || !this.IsRepeating)
                    return this.ExecutionTime - now;
                TimeSpan diff = now - this.ExecutionTime;
                return TimeSpan.FromTicks(this.RepeatInterval.Ticks - diff.Ticks % this.RepeatInterval.Ticks);
            }
        }
        public bool Equals(Reminder? other)
            => other is { } && this.Id == other.Id;

        public override bool Equals(object? obj)
            => this.Equals(obj as Reminder);

        public override int GetHashCode()
            => this.Id.GetHashCode();
    }

}
