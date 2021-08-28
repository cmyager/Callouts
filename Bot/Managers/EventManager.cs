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

namespace Callouts
{
    public class EventManager
    {
        private readonly IDbContextFactory<CalloutsContext> contextFactory;
        private readonly ChannelManager channelManager;
        private readonly GuildManager guildManager;
        private readonly UserManager userManager;
        private readonly UserEventManager userEventManager;
        private readonly DiscordClient client;
        public EventManager(IDbContextFactory<CalloutsContext> contextFactory,
                            ChannelManager channelManager,
                            GuildManager guildManager,
                            UserManager userManager,
                            UserEventManager userEventManager,
                            DiscordClient client)
        {
            this.contextFactory = contextFactory;
            this.channelManager = channelManager;
            this.guildManager = guildManager;
            this.userEventManager = userEventManager;
            this.userManager = userManager;
            this.client = client;
            client.Ready += OnReady;
        }

        public async Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            foreach (var guild in sender.Guilds)
            {
                var guildInfo = guildManager.GetGuild(guild.Value.Id);
                await ListEvents(guild.Value);
            }
            // TODO: Delete events older than X hours
            // TODO: Setup Scheduler for cleaning channel?
            // TODO: Setup reminders? this might be build into the scheduling service
        }

        public async Task ListEvents(DiscordGuild guild)
        {
            await CleanChannel(guild);

            //TODO: Simplify this by comparing the objects more directly. Just trying to get it converted to c# now
            List<Event> dbEvents = await GetDBEvents(guild);
            List<string> dbEventTitles = dbEvents.Select(p => p.Title).ToList();
            List<string> messageTitles = new List<string>();
            var EventsChannel = await channelManager.GetChannel(guild, "upcoming-events");

            foreach (DiscordMessage message in (await EventsChannel.GetMessagesAsync(999)))
            {
                if (IsEventCreateMessage(message))
                {
                    //pass
                }
                if (IsEventMessage(message))
                {
                    string title = message.Embeds[0].Title;
                    if (dbEventTitles.Contains(title))
                    {
                        messageTitles.Add(title);
                    }
                    else
                    {
                        _ = EventsChannel.DeleteMessageAsync(message);
                    }
                }
            }
            List<string> toCreate = dbEventTitles.Where(p => !messageTitles.Contains(p)).ToList();
            if (toCreate.Count > 0)
            {
                foreach (Event e in dbEvents)
                {
                    if (toCreate.Contains(e.Title))
                    {
                        var messagewithembed = await CreateEventMessage(guild, e);
                        await EventsChannel.SendMessageAsync(messagewithembed);
                    }
                }
            }
        }

        public async Task<DiscordMessageBuilder> CreateEventMessage(DiscordGuild guild, Event e)
        {
            string desc = e.Description;
            var time = e.StartTime;
            string timezone = "US/Central";
            DiscordMember creator = await guild.GetMemberAsync(e.UserId);
            // TODO: the userevents list is empty. Is it supposed to be?
            var userEvents = await userEventManager.GetUserEvents(e);

            // TODO: Move this to the event datacontext class
            List<UserEvent> accepted = userEvents.Where(p => p.Attending == UserEventAttending.ACCEPTED).ToList();
            List<UserEvent> declined = userEvents.Where(p => p.Attending == UserEventAttending.DECLINED).ToList();
            List<UserEvent> maybe = userEvents.Where(p => p.Attending == UserEventAttending.MAYBE).ToList();
            int? maxMembers = e.MaxMembers;

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Title = e.Title,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Created by {creator.DisplayName}" },
            };

            if (e.Description != null || e.Description != string.Empty)
            {
                embed.Description = e.Description;
            }
            // TODO: Time is broken
            embed.AddField("Time", $"{e.StartTime.ToString("%A %b %-d, %Y @ %-I:%M %p")} {timezone}", false);


            // TODO: If accepted
            string acceptedString = "__Accepted__";
            string declinedString = "__Declined__";
            string maybeString = "__Maybe__";
            string acceptedValue = "-";

            if (e.MaxMembers != null)
            {
                acceptedString = $"__Accepted({accepted.Count}/{e.MaxMembers})__";
            }
            embed.AddField(acceptedString, acceptedValue);

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
                else if (IsEventMessage(message)){}
                else
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
        public bool IsEventCreateMessage(DiscordMessage message)
        {
            bool isMessage = false;
            if (message.Author.Username.Contains("Callouts") && message.Content.Contains("to create an event."))
            {
                isMessage = true;
            }
            return isMessage;
        }
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

        public async Task<Event> AddEvent(Event newEvent)
        {
            // TODO: Some reporting back to the web about this if it fails? Like return null?
            using var context = contextFactory.CreateDbContext();
            var eventInfo = await context.Events.AsQueryable().FirstOrDefaultAsync(p => p.EventId == newEvent.EventId, cancellationToken: CancellationToken.None);
            if (eventInfo == null)
            {
                context.Add(newEvent);
                await context.SaveChangesAsync();
            }
            _ = ListEvents(await client.GetGuildAsync(newEvent.GuildId));
            return eventInfo;
        }

        public async Task<List<Event>> GetDBEvents(DiscordGuild guild)
        {
            // TODO: make this not  async?
            using var context = contextFactory.CreateDbContext();
            List<Event> events = new List<Event>();
            events = context.Events.AsQueryable().Where(p => p.GuildId == guild.Id).ToList();//, cancellationToken: CancellationToken.None);
            if (events != null)
            {
                return events.ToList();
            }
            return events;
        }
    }
}
