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
    /// <summary>
    /// SchedulingService
    /// </summary>
    public sealed class SchedulingService : IDisposable
    {
        public IServiceProvider serviceProvider;
        public TimeSpan ReloadSpan { get; }
        public DateTimeOffset LastReloadTime { get; private set; }

        private readonly DiscordClient client;
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        private readonly AsyncExecutionService async;
        private readonly ConcurrentDictionary<ulong, ScheduledTaskExecutor> tasks;
        private readonly ConcurrentDictionary<int, ScheduledTaskExecutor> reminders;
        private Timer? loadTimer;

        /// <summary>
        /// SchedulingService
        /// </summary>
        /// <param name="contextFactory"></param>
        /// <param name="client"></param>
        /// <param name="async"></param>
        /// <param name="eventManager"></param>
        /// <param name="start"></param>
        public SchedulingService(IDbContextFactory<CalloutsContext> contextFactory,
                                 DiscordClient client,
                                 AsyncExecutionService async,
                                 IServiceProvider serviceProvider,
                                 bool start = true)
        {
            this.client = client;
            this.ContextFactory = contextFactory;
            this.async = async;
            this.serviceProvider = serviceProvider;
            this.tasks = new ConcurrentDictionary<ulong, ScheduledTaskExecutor>();
            this.reminders = new ConcurrentDictionary<int, ScheduledTaskExecutor>();
            this.LastReloadTime = DateTimeOffset.Now;
            this.ReloadSpan = TimeSpan.FromMinutes(5);
            if (start)
            {
                this.Start();
            }
        }

        /// <summary>
        /// LoadCallback
        /// </summary>
        /// <param name="_"></param>
        private async void LoadCallback(object? _)
        {
            SchedulingService @this = _ as SchedulingService ?? throw new InvalidOperationException();
            await RegisterReminders();
            @this.LastReloadTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// RegisterReminders
        /// </summary>
        /// <returns></returns>
        public async Task RegisterReminders()
        {
            using var context = ContextFactory.CreateDbContext();

            var UpcomingEvents = await context.Events.AsQueryable()
                //.Include(p => p.Guild)
                .Where(t => t.StartTime <= DateTime.UtcNow.AddMinutes(70))
                .ToListAsync();

            // If there is a reminder task and the event is old remove it
            foreach ((int eventId, ScheduledTaskExecutor texec) in this.reminders)
            {
                // TODO: Test this. It might not be needed if the on complete task thing works
                if ((texec.Job as Reminder).EventTime < DateTime.UtcNow)
                {
                    await UnscheduleTask(reminders[eventId].Job, true);
                }
            }
            // TODO Consider: Remove reminders if all people are confirmed?
            // TODO: Remove reminders for events that were deleted?

            foreach (Event upcomingEvent in UpcomingEvents)
            {
                if (!reminders.ContainsKey(upcomingEvent.EventId) && (DateTime.UtcNow - upcomingEvent.StartTime).TotalSeconds < 0)
                {
                    Reminder eventReminder = new()
                    {
                        Id = upcomingEvent.EventId,
                        EventTime = upcomingEvent.StartTime,
                        ExecutionTime = upcomingEvent.StartTime.AddMinutes(-60)
                    };
                    await this.RegisterTask(eventReminder);
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.loadTimer?.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.tasks)
                texec.Dispose();
            foreach ((_, ScheduledTaskExecutor texec) in this.reminders)
                texec.Dispose();
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start()
        {
            this.loadTimer = new Timer(LoadCallback, this, TimeSpan.FromSeconds(1), this.ReloadSpan);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void ScheduleTask(ScheduledTask task)
        {
            ScheduledTaskExecutor? texec = null;
            try
            {
                texec = this.CreateTaskExecutor(task);
            }
            catch (Exception)
            {
                texec?.Dispose();
                //Log.Warning(e, "Scheduling tasks failed");
                throw;
            }
        }

        /// <summary>
        /// UnscheduleTask
        /// </summary>
        /// <param name="task"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        //TODO: Is force needed?
        public Task UnscheduleTask(ScheduledTask task, bool force = false)
        {
            switch (task)
            {
                case FetchReport _:
                    if (this.tasks.TryRemove((task as FetchReport).DiscordUserId, out ScheduledTaskExecutor? taskExec))
                    {
                        taskExec.Dispose();
                    }
                    else
                    {
                        //Log.Warning("Failed to remove guild task from task collection: {GuildTaskId}", task.Id);
                    }
                    break;
                case Reminder rem:
                    //if (!force && rem.IsRepeating && rem.RepeatInterval < this.ReloadSpan)
                    //{
                    //    break;
                    //}
                    if (this.reminders.TryRemove(task.Id, out ScheduledTaskExecutor? remindExec))
                    {
                        remindExec.Dispose();
                    }
                    else
                    {
                        //Log.Warning("Failed to remove reminder from task collection: {ReminderId}", task.Id);
                    }
                    break;
                default:
                    //Log.Warning("Unknown scheduled task type: {ScheduledTaskType}", task.GetType());
                    break;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// RegisterTask
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task<ScheduledTaskExecutor> RegisterTask(ScheduledTask task)
        {
            ScheduledTaskExecutor texec = this.CreateTaskExecutor(task);
            if (task.IsExecutionTimeReached)
            {
                await texec.HandleMissedExecutionAsync();
                await this.UnscheduleTask(task);
                texec = null;
            }
            return texec;
        }

        /// <summary>
        /// ScheduledTaskExecutor
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private ScheduledTaskExecutor CreateTaskExecutor(ScheduledTask task)
        {
            var texec = new ScheduledTaskExecutor(this.client, this.async, task, serviceProvider);
            texec.OnTaskExecuted += this.UnscheduleTask;
            if (this.RegisterExecutor(texec) && !task.IsExecutionTimeReached)
            {
                texec.ScheduleExecution();
            }
            return texec;
        }

        /// <summary>
        /// RegisterExecutor
        /// </summary>
        /// <param name="texec"></param>
        /// <returns></returns>
        private bool RegisterExecutor(ScheduledTaskExecutor texec)
        {
            bool retval = true;
            if (texec.Job is Reminder rem)
            {
                //Log.Debug("Attempting to register reminder {ReminderId} in channel {Channel} @ {ExecutionTime}", rem.Id, rem.ChannelId, rem.ExecutionTime);
                if (!this.reminders.TryAdd(texec.Id, texec))
                {
                    //if (!rem.IsRepeating)
                    //Log.Warning("Reminder {Id} already exists in the collection for user {UserId}", texec.Id, rem.UserId);
                    retval = false;
                }
            }
            else
            {
                //Log.Debug("Attempting to register guild task {ReminderId} @ {ExecutionTime}", texec.Id, texec.Job.ExecutionTime);
                if (!this.tasks.TryAdd((texec.Job as FetchReport).DiscordUserId, texec))
                {
                    //Log.Warning("Guild task {Id} already exists in the collection for user {UserId}", texec.Id);
                    retval = false;
                }
            }
            return retval;
        }
    }
}
