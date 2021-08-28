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
    public class UserEventManager
    {
        private readonly IDbContextFactory<CalloutsContext> contextFactory;
        private readonly ChannelManager channelManager;
        private readonly GuildManager guildManager;
        private readonly UserManager userManager;
        private readonly EventManager eventManager;
        private readonly DiscordClient client;
        public UserEventManager(IDbContextFactory<CalloutsContext> contextFactory,
                            ChannelManager channelManager,
                            GuildManager guildManager,
                            UserManager userManager,
                            EventManager eventManager,
                            DiscordClient client)
        {
            this.contextFactory = contextFactory;
            this.channelManager = channelManager;
            this.guildManager = guildManager;
            this.userManager = userManager;
            this.eventManager = eventManager;
            this.client = client;
            //client.Ready += OnReady;
            client.ComponentInteractionCreated += ComponentInteractionCreatedCallback;
            //client.GuildMemberRemoved += RemoveGuildMemberRemoved;
            //client.GuildMemberAdded += AddGuildMember;
        }

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
                await UpdateAttendance(eventId, e.User, attending);
            }
        }

        private async Task UpdateAttendance(int eventId, DiscordUser discordUser, UserEventAttending attending)
        {
            using var context = contextFactory.CreateDbContext();

            // Get the event
            var associatedEvent = await context.Events.AsQueryable()
                .FirstAsync(p => p.EventId == eventId,
                            cancellationToken: CancellationToken.None);

            // Get the User
            var user = await userManager.GetUserByUserId(discordUser.Id);
            // See if there is a UserEvent
            var userEvent = await context.UserEvents.AsQueryable()
                .FirstOrDefaultAsync(p => p.UserId == discordUser.Id && p.EventId == eventId,
                                     cancellationToken: CancellationToken.None);
            if (userEvent == null)
            {
                userEvent = new UserEvent()
                {
                    EventId = associatedEvent.EventId,
                    Attending = attending,
                    UserId = discordUser.Id,
                    GuildId = associatedEvent.GuildId,
                };
                context.Add(userEvent);
            }
            else
            {
                userEvent.Attending = attending;
                context.Update(userEvent);
            }
            await context.SaveChangesAsync();
            // TODO: Call list here
            _ = eventManager.ListEvents(await client.GetGuildAsync(associatedEvent.GuildId));
        }


        public async Task<List<UserEvent>> GetUserEvents(Event e)
        {
            using var context = contextFactory.CreateDbContext();
            List<UserEvent> userEvents = context.UserEvents.AsQueryable().Where(p => p.EventId == e.EventId).ToList();//, cancellationToken: CancellationToken.None);
            //if (userEvents != null)
            //{
            //    return events.ToList();
            //}
            return userEvents;
        }
    }
}
