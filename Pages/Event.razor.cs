using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.Data;
using Callouts.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;
using Blazorise;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Callouts.Pages
{
    // TODO: Probably rename this page. TOo many things called event
    // TODO: Cleanup. This is a mess since I just copied it from bungie
    [Authorize]
    public partial class Event : ComponentBase
    {
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        UserManager userManager { get; set; }
        [Inject]
        BungieService bungieService { get; set; }
        [Inject]
        AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        UserService userService { get; set; }
        [Inject]
        EventManager eventManager { get; set; }
        [Inject]
        GuildManager guildManager { get; set; }

        private User DiscordUserInfo = null;

        //TODO: Remove my ID. Makes debugging easier for now
        private long userSubmitBungieId { get; set; } = 5396677;
        private UserMembershipData bungieProfile { get; set; }
        private DestinyProfileResponse PrimaryCharacterProfile { get; set; }


        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var discUserClaim = userService.GetInfo(authState);
            if (discUserClaim != null)
            {
                DiscordUserInfo = await userManager.GetUserByUserId(discUserClaim.UserId);
            }
        }
        //CreateEvent
        public async Task CreateEvent()
        {
            ulong guildID = 765038177605386240;
            Guild guild = await guildManager.GetGuild(guildID);
            DataContext.Event newEvent = new DataContext.Event()
            {
                Title = "Test 1 max",
                Description = "TEST",
                MaxMembers = 1,
                //Guild = guild,
                GuildId = guildID,
                //Guild = guild,
                //User = DiscordUserInfo,
                UserId = DiscordUserInfo.UserId,
                StartTime = (DateTime.UtcNow).AddHours(1).AddMinutes(2)
            };
            await eventManager.AddEvent(newEvent);
        }
    }
}
