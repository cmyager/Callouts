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

namespace Callouts
{
    public class EventManager
    {
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
            client.Ready += OnReady;
            client.GuildAvailable += GuildAvailable;
            client.ComponentInteractionCreated += ComponentInteractionCreatedCallback;
        }

        /// <summary>
        /// GuildAvailable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // Make sure the guild exists
            await guildManager.GetGuild(e.Guild.Id);
            // TODO: Make this a scheuled async task once that is ready
            _ = ListEvents(e.Guild);
        }

        /// <summary>
        /// OnReady
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            //foreach (var guild in sender.Guilds)
            //{
            //    // Make sure the guild exists
            //    await guildManager.GetGuild(guild.Value.Id);
            //    await ListEvents(guild.Value);
            //}
            // TODO: Delete events older than X hours
            // TODO: Setup Scheduler for cleaning channel?
            // TODO: Setup reminders? this might be build into the scheduling service
        }

        /// <summary>
        /// ComponentInteractionCreatedCallback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task ComponentInteractionCreatedCallback(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            // Only ack if it is an event button
            if (e.Interaction.Data.CustomId.Contains("_EVENT_"))
            {
                // Respond so it doesn't get mad
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                // Parse the things we need
                string[] splitString = e.Id.Split("_EVENT_");
                UserEventAttending attending = (UserEventAttending)System.Enum.Parse(typeof(UserEventAttending), splitString[1]);
                int eventId = System.Int32.Parse(splitString[0]);
                // Update users attendance
                await UpdateAttendance(eventId, e.User, e.Message, attending);
            }
        }

        /// <summary>
        /// GetEvent
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<Event> GetEvent(int eventId)
        {
            using var context = contextFactory.CreateDbContext();
            // Get the event
            var associatedEvent = await context.Events.AsQueryable()
                .Include(p => p.UserEvents)
                .Include(p => p.Guild)
                .Include(p => p.User)
                .FirstAsync(p => p.EventId == eventId,
                            cancellationToken: CancellationToken.None);
            return associatedEvent;
        }

        private async Task UpdateAttendance(int eventId, DiscordUser discordUser, DiscordMessage discordMessage, UserEventAttending attending)
        {
            using var context = contextFactory.CreateDbContext();

            // Make sure the exists in the database
            var user = await userManager.GetUserByUserId(discordUser.Id);

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
                context.Update(userEvent);
            }
            if (await context.SaveChangesAsync() > 0)
            {
                associatedEvent = await GetEvent(eventId);
                var messagewithembed = await CreateEventMessage(associatedEvent);
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
            var EventsChannel = await channelManager.GetChannel(discordGuild, "upcoming-events");

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

        public async Task<DiscordMessageBuilder> CreateEventMessage(Event e)
        {
            string timezone = "US/Central";
            DiscordGuild guild = await client.GetGuildAsync(e.GuildId);

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Title = e.Title,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Created by {(await guild.GetMemberAsync(e.UserId)).DisplayName}" },
            };

            if (e.Description != null || e.Description != string.Empty)
            {
                embed.Description = e.Description;
            }
            //TODO UTC TO CENTRAL
            embed.AddField("Time", $"{e.StartTime.ToString("dddd MMM dd, yyyy @ hh:mm tt")} {timezone}", false);

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

            var checkmarkEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:"));
            var redXEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":x:"));
            var GreyQuestion = new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":grey_question:"));
            List<DiscordButtonComponent> buttons = new List<DiscordButtonComponent>
            {
                new DiscordButtonComponent(ButtonStyle.Primary, $"{e.EventId}_EVENT_{UserEventAttending.ACCEPTED}", null, false, checkmarkEmoji),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{e.EventId}_EVENT_{UserEventAttending.DECLINED}", null, false, redXEmoji),
                new DiscordButtonComponent(ButtonStyle.Primary, $"{e.EventId}_EVENT_{UserEventAttending.MAYBE}", null, false, GreyQuestion)
            };
            DiscordMessageBuilder message = new DiscordMessageBuilder();
            message.AddEmbed(embed);
            message.AddComponents(buttons);
            return message;
        }

        /// <summary>
        /// CleanChannel
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task CleanChannel(DiscordGuild guild)
        {
            bool EventCreateMessageFound = false;

            var EventsChannel = await channelManager.GetChannel(guild, "upcoming-events");
            foreach (DiscordMessage message in (await EventsChannel.GetMessagesAsync(999)))
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
                _ = EventsChannel.SendMessageAsync(builder);
            }
        }

        // TODO: Might merge these into one method. In the old python bot it was 3 because there was a purge method
        public bool PurgeEventMessageCheck(DiscordMessage message)
        {
            bool deleteMessage = true;
            if (IsEventCreateMessage(message) || IsEventMessage(message))
            {
                deleteMessage = false;
            }
            return deleteMessage;
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
            if (message.Channel.Name == "upcoming-events" && message.Embeds.Count > 0 && message.Embeds[0].Fields.Count >= 3)
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
    }
}
