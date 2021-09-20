using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Callouts;
using Callouts.DataContext;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Callouts.Data
{
    public class ScheduledTaskExecutor : IDisposable
    {
        public IServiceProvider serviceProvider;
        public int Id => this.Job.Id;
        public ScheduledTask Job { get; }

        public delegate Task TaskExecuted(ScheduledTask task, bool force);
        public event TaskExecuted OnTaskExecuted;

        private readonly DiscordClient client;
        private readonly AsyncExecutionService async;
        private Timer? timer;
        private int callbackCount = 0;

        /// <summary>
        /// ScheduledTaskExecutor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="async"></param>
        /// <param name="task"></param>
        /// <param name="serviceProvider"></param>
        public ScheduledTaskExecutor(DiscordClient client, AsyncExecutionService async, ScheduledTask task, IServiceProvider serviceProvider)
        {
            this.client = client;
            this.async = async;
            this.Job = task;
            this.serviceProvider = serviceProvider;
            this.OnTaskExecuted += (task, _) => Task.CompletedTask;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
            => this.timer?.Dispose();

        /// <summary>
        /// ScheduleExecution
        /// </summary>
        public void ScheduleExecution()
        {
            switch (this.Job)
            {
                case FetchReport fetch:
                    this.timer = new Timer(this.FetchReportCallback, this.Job, fetch.TimeUntilExecution, fetch.RepeatInterval.Value);
                    break;
                case Reminder rem:
                    this.timer = new Timer(this.SendReminderCallback, this.Job, rem.TimeUntilExecution, rem.RepeatInterval.Value);
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.Job));
            }
        }

        /// <summary>
        /// HandleMissedExecutionAsync
        /// </summary>
        /// <returns></returns>
        public Task HandleMissedExecutionAsync()
        {
            try
            {
                switch (this.Job)
                {
                    case FetchReport fetch:
                        FetchReportCallback(fetch);
                        break;
                    case Reminder rem:
                        SendReminderCallback(rem);
                        break;
                    default:
                        throw new ArgumentException("Unknown saved task info type!", nameof(this.Job));
                }
                //Log.Debug("Executed missed saved task of type: {SavedTaskType}", this.Job.GetType().Name);
            }
            catch (Exception)
            {
                // TODO: Figure out logging
                //Log.Debug(e, "Error while handling missed saved task");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// SendMessageCallback
        /// </summary>
        /// <param name="_"></param>
        private void SendReminderCallback(object? _)
        {
            // TODO: It might be useful to remove the old buttons from the previous reminders
            callbackCount += 1;
            Reminder? rem = _ as Reminder ?? throw new InvalidCastException("Failed to cast scheduled task to Reminder");
            EventManager eventManager = serviceProvider.GetRequiredService<EventManager>();
            Event upcomingEvent = this.async.Execute(eventManager.GetEvent(rem.Id));
            DiscordGuild guild = this.async.Execute(client.GetGuildAsync(upcomingEvent.GuildId));

            foreach (UserEvent accepteduser in upcomingEvent.Accepted.Concat(upcomingEvent.Standby))
            {
                if (accepteduser.Attending == UserEventAttending.CONFIRMED)
                {
                    continue;
                }
                DiscordMember member = this.async.Execute(guild.GetMemberAsync(accepteduser.UserId));
                accepteduser.AddAttempt();
                DiscordMessageBuilder reminderMessage = this.async.Execute(eventManager.CreateEventReminderMessage(accepteduser));
                this.async.Execute(member.SendMessageAsync(reminderMessage));
                this.async.Execute(eventManager.UpdateAttendance(upcomingEvent.EventId, member.Id, accepteduser.Attending, accepteduser.Attempts));
            }
            if (callbackCount >= 5)
            {
                this.async.Execute(this.OnTaskExecuted(this.Job, false));
            }
        }

        /// <summary>
        /// FetchReportCallback
        /// </summary>
        /// <param name="_"></param>
        private void FetchReportCallback(object? _)
        {
            FetchReport? fetch = _ as FetchReport ?? throw new InvalidCastException("Failed to cast scheduled task to FetchReport");

            ReportManager reportManager = serviceProvider.GetRequiredService<ReportManager>();
            this.async.Execute(reportManager.GetReport(fetch.DiscordUserId, fetch.GuildId, fetch.filter));

            if (!fetch.IsRepeating)
            {
                this.async.Execute(this.OnTaskExecuted(this.Job, false));
            }
        }
    }
}
