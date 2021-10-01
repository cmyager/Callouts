using Blazorise;
using Callouts.Data;
using Callouts.DataContext;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callouts.Pages
{
    // TODO: Probably rename this page. TOo many things called event
    // TODO: Cleanup. This is a mess since I just copied it from bungie
    [Authorize]
    public partial class CreateEvent : ComponentBase
    {
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        UserManager userManager { get; set; }
        [Inject]
        AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        UserService userService { get; set; }
        [Inject]
        EventManager eventManager { get; set; }
        [Inject]
        GuildManager guildManager { get; set; }

        Alert EventAlert;
        private User DiscordUserInfo = null;
        private string? Title;
        private string? Description;
        private DateTime Date;
        private TimeSpan Time;//SET THESE IN ON INIT
        private int? Attendees = null;
        private string? ErrorMessage = null;

        DiscordGuild SelectedGuild = null;
        //List<DiscordMember> selectedMembers = new();

        List<DiscordGuild> UserDiscordGuild = new List<DiscordGuild>();
        //List<DiscordMember> DiscordMembers = new List<DiscordMember>();

        protected override async Task OnInitializedAsync()
        {
            TimeZoneInfo CST = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime newDT = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CST);
            Date = newDT.Date;
            Time = newDT.TimeOfDay;

            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var discUserClaim = userService.GetInfo(authState);
            if (discUserClaim != null)
            {
                DiscordUserInfo = await userManager.GetUserByUserId(discUserClaim.UserId);
                UserDiscordGuild = await guildManager.GetGuildsFromUserId(discUserClaim.UserId);
                SelectedGuild = UserDiscordGuild.FirstOrDefault();
                //await UpdateMemberList();
            }
        }

        //private async Task UpdateMemberList()
        //{
        //    DiscordMembers = new();
        //    selectedMembers = new();
        //    if (SelectedGuild != null)
        //    {
        //        DiscordMembers = await guildManager.GetGuildMembersFromGuildId(SelectedGuild.Id);
        //    }
        //    StateHasChanged();
        //}

        public async Task CreateNewEvent()
        {
            EventAlert.Hide();
            ErrorMessage = null;

            TimeZoneInfo CST = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime UtcTime = TimeZoneInfo.ConvertTimeToUtc(Date.Add(Time), CST);

            if (SelectedGuild == null)
            {
                ErrorMessage = "No guild Selected";
            }
            else if (Title == null || Title.Length == 0)
            {
                ErrorMessage = "Title Error";
            }
            else if (UtcTime < DateTime.UtcNow)
            {
                // TODO: Fix this to compare properly
                ErrorMessage = "Date Error";
            }
            else if (Attendees != null && Attendees.Value < 0)
            {
                ErrorMessage = "Attendees error";
            }
            else if (Description == null)
            {
                Description = "";
            }

            if (ErrorMessage != null)
            {
                EventAlert.Show();
            }
            else
            {
                //ulong guildID = 765038177605386240;

                Event newEvent = new()
                {
                    Title = Title,
                    Description = Description,
                    MaxMembers = Attendees,
                    //Guild = guild,
                    GuildId = SelectedGuild.Id,
                    //Guild = guild,
                    //User = DiscordUserInfo,
                    UserId = DiscordUserInfo.UserId,
                    StartTime = UtcTime
                };
                await eventManager.AddEvent(newEvent);
                ErrorMessage = "Event should have been created!";
                EventAlert.Show();
            }
        }

        public async void GuildChanged(ChangeEventArgs e)
        {
            SelectedGuild = UserDiscordGuild.FirstOrDefault(p => p.Id.ToString() == e.Value.ToString());
            StateHasChanged();
        }
    }
}
