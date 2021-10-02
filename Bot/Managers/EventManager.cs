using BungieSharper.Entities;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Linq;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System;

// TODO: Break this out into EventManager and UserEventManager?
namespace Callouts
{
    public class EventManager
    {
        private readonly string EventDeliminator = "_EVENT_";
        private readonly string EventReminderDeliminator = "_EVENTREMINDER_";
        private readonly string EventChannelName = "upcoming-events";

        private readonly string DeleteEmojiName = ":skull:";
        private readonly DiscordEmoji DeleteEmoji;

        private readonly IDbContextFactory<CalloutsContext> contextFactory;
        private readonly ChannelManager channelManager;
        private readonly GuildManager guildManager;
        private readonly UserManager userManager;
        private readonly DiscordClient client;
        public EventManager(IDbContextFactory<CalloutsContext> contextFactory,
                            ChannelManager channelManager,
                            GuildManager guildManager,
                            UserManager userManager,
                            DiscordClient client)
        {
            this.contextFactory = contextFactory;
            this.channelManager = channelManager;
            this.guildManager = guildManager;
            this.userManager = userManager;
            this.client = client;

            DeleteEmoji = DiscordEmoji.FromName(client, DeleteEmojiName);
            client.Ready += OnReady;
            client.ComponentInteractionCreated += ComponentInteractionCreatedCallback;
            client.MessageReactionAdded += MessageReactionAdded;
        }

        /// <summary>
        /// MessageReactionAdded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            var completeMessage = await e.Channel.GetMessageAsync(e.Message.Id);
            var user = await e.Guild.GetMemberAsync(e.User.Id);

            // if it is an event message, the emoji is the delete emoji
            // and the user is the event creator or an admin delete it.
            if (IsEventMessage(completeMessage)
                && e.Emoji == DeleteEmoji
                && (user.Id == completeMessage.Id
                    || user.Permissions >= Permissions.Administrator))
            {
                Event toDeleteEvent = await GetEvent(null, e.Guild.Id, completeMessage.Embeds[0].Title);
                _ = DeleteEvent(toDeleteEvent);
            }
            else
            {
                // Remove extra emojis
                _ = completeMessage.DeleteReactionsEmojiAsync(e.Emoji);
            }
        }

        /// <summary>
        /// OnReady
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            channelManager.AddRequiredChannel(EventChannelName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// ComponentInteractionCreatedCallback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task ComponentInteractionCreatedCallback(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            // TODO: This can probably be simplified since both cases do pretty much the same actions
            // Only ack if it is an event button
            if (e.Interaction.Data.CustomId.Contains(EventDeliminator))
            {
                // Respond so it doesn't get mad
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                // Parse the things we need
                string[] splitString = e.Id.Split(EventDeliminator);
                UserEventAttending attending = (UserEventAttending)System.Enum.Parse(typeof(UserEventAttending), splitString[1]);
                int eventId = System.Int32.Parse(splitString[0]);
                // Update users attendance
                await UpdateAttendance(eventId, e.User.Id, attending, discordMessage: e.Message);
            }
            else if (e.Interaction.Data.CustomId.Contains(EventReminderDeliminator))
            {
                // Respond so it doesn't get mad
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                // Parse the things we need
                string[] splitString = e.Id.Split(EventReminderDeliminator);
                UserEventAttending attending = UserEventAttending.CONFIRMED;
                int eventId = System.Int32.Parse(splitString[0]);
                await UpdateAttendance(eventId, e.User.Id, attending);

            }
        }

        /// <summary>
        /// GetEvent
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<Event> GetEvent(int? eventId = null, ulong? guildId = null, string? title = null)
        {
            using var context = contextFactory.CreateDbContext();

            IQueryable<Event> EventQuery = context.Events.AsQueryable();
            if (eventId != null)
            {
                EventQuery = EventQuery.Where(p => p.EventId == eventId);
            }
            if (guildId != null)
            {
                EventQuery = EventQuery.Where(p => p.GuildId == guildId);
            }
            if (title != null)
            {
                EventQuery = EventQuery.Where(p => p.Title == title);
            }
            EventQuery = EventQuery.Include(p => p.UserEvents);
            EventQuery = EventQuery.Include(p => p.Guild);
            EventQuery = EventQuery.Include(p => p.User);

            // Get the event
            var associatedEvent = await EventQuery.FirstAsync(cancellationToken: CancellationToken.None);
            return associatedEvent;
        }

        /// <summary>
        /// GetUserEvent
        /// </summary>
        /// <param name="userEventId"></param>
        /// <returns></returns>
        public async Task<UserEvent> GetUserEvent(int userEventId)
        {
            using var context = contextFactory.CreateDbContext();

            IQueryable<UserEvent> UserEventQuery = context.UserEvents.AsQueryable();
            UserEventQuery = UserEventQuery.Where(p => p.UserEventId == userEventId);

            UserEventQuery = UserEventQuery.Include(p => p.Event);
            UserEventQuery = UserEventQuery.Include(p => p.Guild);
            UserEventQuery = UserEventQuery.Include(p => p.User);

            // Get the UserEvent
            var associatedUserEvent = await UserEventQuery.FirstAsync(cancellationToken: CancellationToken.None);
            return associatedUserEvent;
        }

        /// <summary>
        /// associatedEvent
        /// </summary>
        /// <param name="associatedEvent"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetEventMessage(Event associatedEvent)
        {
            // TODO: This can slow down the process quite a bit.
            DiscordGuild guild = await client.GetGuildAsync(associatedEvent.GuildId);
            var EventsChannel = await channelManager.GetChannel(guild, EventChannelName);
            DiscordMessage foundMessage = null;
            //await ListEvents(guild); // TODO: This slows it down real bad

            foreach (DiscordMessage message in (await EventsChannel.GetMessagesAsync(50)))
            {
                if (IsEventMessage(message) && message.Embeds[0].Title == associatedEvent.Title)
                {
                    foundMessage = message;
                    break;
                }
            }
            return foundMessage;
        }

        /// <summary>
        /// UpdateAttendance
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="discordUser"></param>
        /// <param name="discordMessage"></param>
        /// <param name="attending"></param>
        /// <returns></returns>
        public async Task UpdateAttendance(int eventId, ulong userId, UserEventAttending attending, int? attempts=null, DiscordMessage discordMessage=null)
        {
            using var context = contextFactory.CreateDbContext();

            // Make sure the exists in the database
            var user = await userManager.GetUserByUserId(userId);

            // Get the event
            var associatedEvent = await GetEvent(eventId);

            // See if there is a UserEvent
            UserEvent userEvent = associatedEvent.UserEvents
                .Where(p => p.UserId == user.UserId)
                .FirstOrDefault();

            if (userEvent == null)
            {
                userEvent = new UserEvent()
                {
                    Attending = attending,
                    EventId = associatedEvent.EventId,
                    GuildId = associatedEvent.GuildId,
                    Title = associatedEvent.Title,
                    UserId = user.UserId
                };
                context.Add(userEvent);
            }
            else
            {
                userEvent.Attending = attending;
                if (userEvent.Attending != UserEventAttending.ACCEPTED && userEvent.Attending != UserEventAttending.CONFIRMED)
                {
                    userEvent.LastUpdated = DateTime.UtcNow;
                }
                if (attempts != null)
                {
                    userEvent.Attempts = attempts.Value;
                }
                context.Update(userEvent);
            }
            if (await context.SaveChangesAsync() > 0)
            {
                associatedEvent = await GetEvent(eventId);
                var messagewithembed = await CreateEventMessage(associatedEvent);
                if (discordMessage == null)
                {
                    discordMessage = await GetEventMessage(associatedEvent);  // TODO: This slows it down real bad
                }
                await discordMessage.ModifyAsync(messagewithembed.Embed);
                _ = ListEvents(await client.GetGuildAsync(associatedEvent.GuildId));
            }
        }

        /// <summary>
        /// ListEvents
        /// </summary>
        /// <param name="discordGuild"></param>
        /// <returns></returns>
        public async Task ListEvents(DiscordGuild discordGuild)
        {
            await CleanChannel(discordGuild);
            Guild guild = await guildManager.GetGuild(discordGuild.Id);

            var dbEventTitles = guild.Events.Select(p => p.Title).ToList();
            var messageTitles = new List<string>();
            var EventsChannel = await channelManager.GetChannel(discordGuild, EventChannelName);

            foreach (DiscordMessage message in (await EventsChannel.GetMessagesAsync(999)))
            {
                // if (!IsEventCreateMessage(message) || IsEventMessage(message))
                if (IsEventMessage(message))
                {
                    string title = message.Embeds[0].Title;
                    if (dbEventTitles.Contains(title))
                    {
                        messageTitles.Add(title);
                    }
                    else
                    {
                        // Delete old event message
                        _ = EventsChannel.DeleteMessageAsync(message);
                    }
                }
            }
            var toCreate = dbEventTitles.Where(p => !messageTitles.Contains(p));
            var eventsToCreate = guild.Events.Where(e => toCreate.Contains(e.Title));
            foreach (Event e in eventsToCreate)
            {
                var messagewithembed = await CreateEventMessage(e);
                await EventsChannel.SendMessageAsync(messagewithembed);
            }
        }

        /// <summary>
        /// CreateEventMessage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task<DiscordMessageBuilder> CreateEventMessage(Event e)
        {
            DiscordEmbedBuilder eventEmbed = await CreateEventEmbed(e);

            var checkmarkEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:"));
            var redXEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":x:"));
            var GreyQuestion = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":grey_question:"));
            List<DiscordButtonComponent> buttons = new List<DiscordButtonComponent>
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{e.EventId}{EventDeliminator}{UserEventAttending.ACCEPTED}", null, false, checkmarkEmoji),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{e.EventId}{EventDeliminator}{UserEventAttending.DECLINED}", null, false, redXEmoji),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{e.EventId}{EventDeliminator}{UserEventAttending.MAYBE}", null, false, GreyQuestion)
            };
            DiscordMessageBuilder message = new DiscordMessageBuilder();
            message.AddEmbed(eventEmbed);
            message.AddComponents(buttons);
            return message;
        }

        /// <summary>
        /// CreateEventReminderMessage
        /// </summary>
        /// <param name="userEvent"></param>
        /// <returns></returns>
        public async Task<DiscordMessageBuilder> CreateEventReminderMessage(UserEvent userEvent)
        {
            DiscordMessageBuilder message = new DiscordMessageBuilder()
            {
                Content = "You have been removed from the event"
            };
            if (userEvent.Attending == UserEventAttending.ACCEPTED)
            {
                DiscordEmbedBuilder eventEmbed = await CreateEventReminderEmbed(userEvent);
                var checkmarkEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:"));
                List<DiscordButtonComponent> buttons = new List<DiscordButtonComponent>
                {
                    // TODO: Think about the button ID structure. Attempts is not really needed
                    new DiscordButtonComponent(ButtonStyle.Primary, $"{userEvent.UserEventId}{EventReminderDeliminator}{userEvent.Attempts}", null, false, checkmarkEmoji),
                };
                message.Content = "";
                message.AddEmbed(eventEmbed);
                message.AddComponents(buttons);
            }
            return message;
        }

        /// <summary>
        /// CreateEventMessage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task<DiscordEmbedBuilder> CreateEventEmbed(Event e)
        {
            DiscordGuild guild = await client.GetGuildAsync(e.GuildId);

            string footerText = $"Created by {(await guild.GetMemberAsync(e.UserId)).DisplayName}\n";
            footerText += $"React with {DeleteEmoji} to remove this event";

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Title = e.Title,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = footerText },
            };

            if (e.Description != null || e.Description != string.Empty)
            {
                embed.Description = e.Description;
            }

            embed.AddField("Time", $"{e.StartTime.UtcToCst():dddd MMM dd, yyyy @ hh:mm tt} US/Central", false);

            string acceptedString = e.MaxMembers != null ? $"__Accepted({e.Accepted.Count}/{e.MaxMembers})__" : "__Accepted__";
            string declinedString = "__Declined__";
            string maybeString = "__Maybe__";
            string standbyString = "__Standby__";

            string acceptedValue = e.Accepted.Any() ? "" : "-";
            string declinedValue = e.Declined.Any() ? "" : "-";
            string maybeValue = e.Maybe.Any() ? "" : "-";
            string standbyValue = e.Standby.Any() ? "" : "-";

            foreach (var user in e.Accepted)
            {
                string discordUserName = (await guild.GetMemberAsync(user.UserId)).DisplayName;
                if (user.Attending == UserEventAttending.CONFIRMED)
                {
                    discordUserName = $"**{discordUserName}**";
                }
                acceptedValue += $"{discordUserName}\n";
            }

            foreach (var user in e.Declined)
            {
                string discordUserName = (await guild.GetMemberAsync(user.UserId)).DisplayName;
                if (user.Attending == UserEventAttending.REJECTED)
                {
                    discordUserName = $"~~{discordUserName}~~";
                }
                declinedValue += $"{discordUserName}\n";
            }

            foreach (var user in e.Maybe)
            {
                string discordUserName = (await guild.GetMemberAsync(user.UserId)).DisplayName;
                maybeValue += $"{discordUserName}\n";
            }

            foreach (var user in e.Standby)
            {
                string discordUserName = (await guild.GetMemberAsync(user.UserId)).DisplayName;
                if (user.Attending == UserEventAttending.CONFIRMED)
                {
                    discordUserName = $"**{discordUserName}**";
                }
                standbyValue += $"{discordUserName}\n";
            }

            embed.AddField(acceptedString, acceptedValue, true);
            embed.AddField(declinedString, declinedValue, true);
            embed.AddField(maybeString, maybeValue, true);
            if (e.Standby.Any())
            {
                embed.AddField(standbyString, standbyValue, false);
            }
            return embed;
        }

        /// <summary>
        /// CreateEventReminderEmbed
        /// </summary>
        /// <param name="userEvent"></param>
        /// <returns></returns>
        public async Task<DiscordEmbedBuilder> CreateEventReminderEmbed(UserEvent userEvent)
        {
            Event completeEvent = await GetEvent(userEvent.EventId);
            DiscordEmbedBuilder eventReminderEmbed = await CreateEventEmbed(completeEvent);
            DiscordGuild guild = await client.GetGuildAsync(userEvent.GuildId);

            string footerText = $"Please confirm your attendance.\nAttempt {userEvent.Attempts}/5";
            eventReminderEmbed.Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = footerText };

            string titleHold = eventReminderEmbed.Title;
            DiscordEmbedField timeHold = eventReminderEmbed.Fields[0];
            string newTitle = "Event Reminder";
            if (userEvent.IsStandby)
            {
                newTitle = $"{newTitle} (Standby)";
            }
            eventReminderEmbed.Title = newTitle;
            eventReminderEmbed.RemoveFieldRange(0, 4);
            eventReminderEmbed.AddField("Server", guild.Name, false);
            eventReminderEmbed.AddField("Event", titleHold, false);
            eventReminderEmbed.AddField(timeHold.Name, timeHold.Value, false);
            return eventReminderEmbed;
        }

        /// <summary>
        /// CleanChannel
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task CleanChannel(DiscordGuild guild)
        {
            using var context = contextFactory.CreateDbContext();
            bool EventCreateMessageFound = false;

            // Remove events older than 1 day
            var EventQuery = await context.Events.AsQueryable()
                .Where(p => p.GuildId == guild.Id).ToListAsync();
            foreach (Event e in EventQuery)
            {
                if (DateTime.UtcNow - e.StartTime > TimeSpan.FromHours(24))
                {
                    await DeleteEvent(e, listEvents: false);
                }
            }

            var EventsChannel = await channelManager.GetChannel(guild, EventChannelName);
            foreach (DiscordMessage message in (await EventsChannel.GetMessagesAsync()))
            {
                if (IsEventCreateMessage(message))
                {
                    EventCreateMessageFound = true;
                }
                else if (!IsEventMessage(message))
                {
                    _ = EventsChannel.DeleteMessageAsync(message);
                }
            }
            if (!EventCreateMessageFound)
            {
                var builder = new DiscordMessageBuilder();
                var button = new DiscordLinkButtonComponent("https://theclanwithoutaplan.com", "https://theclanwithoutaplan.com");
                builder.Content = "Visit the website to create an event.";
                builder.AddComponents(new List<DiscordComponent>() { button });
                await (await EventsChannel.SendMessageAsync(builder)).PinAsync();
            }
        }

        /// <summary>
        /// IsEventCreateMessage
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsEventCreateMessage(DiscordMessage message)
        {
            bool isMessage = false;
            if (message.Author.Username.Contains("Callouts") && message.Content.Contains("to create an event."))
            {
                isMessage = true;
            }
            return isMessage;
        }

        /// <summary>
        /// IsEventMessage
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsEventMessage(DiscordMessage message)
        {
            bool IsMessage = false;
            if (message.Channel.Name == EventChannelName && message.Embeds.Count > 0 && message.Embeds[0].Fields.Count >= 3)
            {
                var embed = message.Embeds[0];
                IsMessage = (embed.Fields[0].Name == "Time"
                          && embed.Fields[1].Name.StartsWith("__Accepted")
                          && embed.Fields[2].Name.StartsWith("__Declined"));
            }
            return IsMessage;
        }

        /// <summary>
        /// AddEvent
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        public async Task<Event> AddEvent(Event newEvent)
        {
            // TODO: Some reporting back to the web about this if it fails? Like return null?
            using var context = contextFactory.CreateDbContext();
            var eventInfo = await context.Events.AsQueryable()
                .FirstOrDefaultAsync(p => p.GuildId == newEvent.GuildId && p.Title == newEvent.Title,
                                     cancellationToken: CancellationToken.None);
            if (eventInfo == null)
            {
                context.Add(newEvent);
                // TODO: Add RSVP people here
                await context.SaveChangesAsync();
                _ = ListEvents(await client.GetGuildAsync(newEvent.GuildId));
            }
            return eventInfo;
        }

        /// <summary>
        /// DeleteEvent
        /// </summary>
        /// <param name="deleteEvent"></param>
        /// <returns></returns>
        public async Task DeleteEvent(Event deleteEvent, bool listEvents=true)
        {
            using var context = contextFactory.CreateDbContext();
            if (deleteEvent != null)
            {
                context.Remove(deleteEvent);
                await context.SaveChangesAsync();
                if (listEvents)
                {
                    _ = ListEvents(await client.GetGuildAsync(deleteEvent.GuildId));
                }
            }
        }

    }
}
