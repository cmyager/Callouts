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
    public abstract class ScheduledTask : IEquatable<ScheduledTask>
    {
        public int Id { get; set; }
        public abstract bool IsRepeating { get; set; }
        public abstract TimeSpan? RepeatInterval { get; set; }
        public DateTimeOffset ExecutionTime { get; set; }
        public bool IsExecutionTimeReached => this.TimeUntilExecution.CompareTo(TimeSpan.Zero) < 0;
        public abstract TimeSpan TimeUntilExecution { get; }
        public bool Equals(ScheduledTask? other) => other is { } && this.Id == other.Id;
        public override bool Equals(object? obj) => this.Equals(obj as ScheduledTask);
        public override int GetHashCode() => this.Id.GetHashCode();

    }
    public class Reminder : ScheduledTask, IEquatable<Reminder>
    {
        // Class specific
        public DateTime EventTime { get; set; }

        // Overrides
        public override bool IsRepeating { get; set; } = true;
        public override TimeSpan? RepeatInterval { get; set; } = TimeSpan.FromMinutes(10);
        public override TimeSpan TimeUntilExecution
        {
            get
            {
                DateTimeOffset now = DateTimeOffset.Now;
                if (this.ExecutionTime > now || !this.IsRepeating)
                {
                    return this.ExecutionTime - now;
                }
                TimeSpan diff = now - this.ExecutionTime;
                return TimeSpan.FromTicks(this.RepeatInterval.Value.Ticks - diff.Ticks % this.RepeatInterval.Value.Ticks);
            }
        }

        public bool Equals(Reminder? other)
            => other is { } && this.Id == other.Id;
        public override bool Equals(object? obj)
            => this.Equals(obj as Reminder);
        public override int GetHashCode()
            => this.Id.GetHashCode();
    }

    /// <summary>
    /// FetchReport
    /// </summary>
    public class FetchReport : ScheduledTask, IEquatable<FetchReport>
    {
        // Class specific
        public ulong? GuildId { get; set; }
        public ulong DiscordUserId { get; set; }
        public bool filter { get; set; } = false;

        // Overrides
        public override bool IsRepeating { get; set; } = false;
        public override TimeSpan? RepeatInterval { get; set; } = TimeSpan.FromMinutes(1);

        public override TimeSpan TimeUntilExecution
        {
            get
            {
                TimeSpan retval = new TimeSpan(0);
                if (this.IsRepeating && this.RepeatInterval != null)
                {
                    TimeSpan diff = DateTimeOffset.Now - this.ExecutionTime;
                    retval = TimeSpan.FromTicks(this.RepeatInterval.Value.Ticks - diff.Ticks % this.RepeatInterval.Value.Ticks);
                }
                return retval;
            }
        }

        public bool Equals(FetchReport? other)
            => other is { } && this.Id == other.Id;
        public override bool Equals(object? obj)
            => this.Equals(obj as FetchReport);
        public override int GetHashCode()
            => this.Id.GetHashCode();
    }

}
