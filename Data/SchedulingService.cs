using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using BungieSharper.Entities;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.DataContext;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

namespace Callouts.Data
{
    public sealed class SchedulingService : IDisposable
    {
        private static void LoadCallback(object? _)
        {
            SchedulingService @this = _ as SchedulingService ?? throw new InvalidOperationException();


            @this.LastReloadTime = DateTimeOffset.Now;


        }


        public bool IsDisabled => false;
        public TimeSpan ReloadSpan { get; }
        public DateTimeOffset LastReloadTime { get; private set; }

        private readonly DiscordClient client;
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        private readonly AsyncExecutionService async;
        private readonly ConcurrentDictionary<int, ScheduledTaskExecutor> tasks;
        private readonly ConcurrentDictionary<int, ScheduledTaskExecutor> reminders;
        private Timer? loadTimer;

        public SchedulingService(IDbContextFactory<CalloutsContext> contextFactory, DiscordClient client, AsyncExecutionService async, bool start = true)
        {
            this.client = client;
            this.ContextFactory = contextFactory;
            this.async = async;
            this.tasks = new ConcurrentDictionary<int, ScheduledTaskExecutor>();
            this.reminders = new ConcurrentDictionary<int, ScheduledTaskExecutor>();
            this.LastReloadTime = DateTimeOffset.Now;
            this.ReloadSpan = TimeSpan.FromMinutes(5);
            if (start)
            {
                this.Start();
            }
        }

        public void Dispose()
        {
            this.loadTimer?.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.tasks)
                texec.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.reminders)
                texec.Dispose();
        }

        public void Start()
        {
            this.loadTimer = new Timer(LoadCallback, this, TimeSpan.FromSeconds(10), this.ReloadSpan);
        }


        public async Task ScheduleAsync(ScheduledTask task)
        {
            ScheduledTaskExecutor? texec = null;
            try
            {
                if (DateTimeOffset.Now + task.TimeUntilExecution <= this.LastReloadTime + this.ReloadSpan)
                    texec = this.CreateTaskExecutor(task);
            }
            catch (Exception e)
            {
                texec?.Dispose();
                //Log.Warning(e, "Scheduling tasks failed");
                throw;
            }
        }

        public async Task UnscheduleAsync(ScheduledTask task, bool force = false)
        {
        }

        public async Task UnscheduleRemindersForUserAsync(ulong uid)
        {
        }

        public async Task UnscheduleRemindersForChannelAsync(ulong cid)
        {
        }

        public async Task<IReadOnlyList<Reminder>> GetRemindTasksForUserAsync(ulong uid)
        {
            List<Reminder> reminders = new List<Reminder>(); // I added new
            return reminders.AsReadOnly();
        }

        public async Task<IReadOnlyList<Reminder>> GetRemindTasksForChannelAsync(ulong cid)
        {
            List<Reminder> reminders = new List<Reminder>(); // I added new
            return reminders.AsReadOnly();
        }


        private async Task<bool> RegisterDbTaskAsync(ScheduledTask task)
        {
            ScheduledTaskExecutor texec = this.CreateTaskExecutor(task);
            if (task.IsExecutionTimeReached)
            {
                await texec.HandleMissedExecutionAsync();
                await this.UnscheduleAsync(task);
                return false;
            }
            return true;
        }

        private ScheduledTaskExecutor CreateTaskExecutor(ScheduledTask task)
        {
            var texec = new ScheduledTaskExecutor(this.client, this.async, task);
            texec.OnTaskExecuted += this.UnscheduleAsync;
            if (this.RegisterExecutor(texec) && !task.IsExecutionTimeReached)
                texec.ScheduleExecution();
            return texec;
        }

        private bool RegisterExecutor(ScheduledTaskExecutor texec)
        {
            if (texec.Job is Reminder rem)
            {
                //Log.Debug("Attempting to register reminder {ReminderId} in channel {Channel} @ {ExecutionTime}", rem.Id, rem.ChannelId, rem.ExecutionTime);
                if (!this.reminders.TryAdd(texec.Id, texec))
                {
                    //if (!rem.IsRepeating)
                        //Log.Warning("Reminder {Id} already exists in the collection for user {UserId}", texec.Id, rem.UserId);
                    return false;
                }
            }
            else
            {
                //Log.Debug("Attempting to register guild task {ReminderId} @ {ExecutionTime}", texec.Id, texec.Job.ExecutionTime);
                if (!this.tasks.TryAdd(texec.Id, texec))
                {
                    //Log.Warning("Guild task {Id} already exists in the collection for user {UserId}", texec.Id);
                    return false;
                }
            }
            return true;
        }
    }
}
