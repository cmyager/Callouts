﻿using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.Data;
using Callouts.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;

namespace Callouts.Pages
{
    [Authorize]
    public partial class Bungie : ComponentBase
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

        private User DiscordUserInfo = null;

        //TODO: Remove my ID. Makes debugging easier for now
        private long userSubmitBungieId { get; set; } = 5396677;
        private UserMembershipData bungieProfile { get; set; }
        private DestinyProfileResponse PrimaryCharacterProfile { get; set; }

        Alert AccountErrorAlert;
        Alert UnlinkAlert;
        Alert CharacterErrorAlert;

        private string alertText = "booba";


        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var discUserClaim = userService.GetInfo(authState);
            if (discUserClaim != null)
            {
                DiscordUserInfo = await userManager.GetUserByUserId(discUserClaim.UserId);
                if (DiscordUserInfo.BungieId != null)
                {
                    bungieProfile = await bungieService.GetUserById(DiscordUserInfo.BungieId.Value, BungieMembershipType.BungieNext);
                    await GetBungieProfile();
                }
            }
        }

        public async Task GetBungieProfile()
        {
            AccountErrorAlert.Hide();
            UnlinkAlert.Hide();
            CharacterErrorAlert.Hide();
            CharacterErrorAlert.Show();
            if (bungieProfile == null)
            {
                bungieProfile = await bungieService.GetUserById(userSubmitBungieId, BungieMembershipType.BungieNext);
            }

            if (bungieProfile == null)
            {
                alertText = "null";
                AccountErrorAlert.Show();
            }
            else if (bungieProfile.DestinyMemberships == null || bungieProfile.DestinyMemberships.Count() == 0)
            {
                alertText = "count 0";
                CharacterErrorAlert.Show();
            }
            else
            {
                long primaryMembershipId = bungieProfile.PrimaryMembershipId.GetValueOrDefault();
                BungieMembershipType primaryMembershipType;

                if (bungieProfile.PrimaryMembershipId == null)
                {
                    primaryMembershipType = bungieProfile.DestinyMemberships.First().MembershipType;
                }
                else
                {
                    primaryMembershipType = bungieProfile.DestinyMemberships.First(p => p.MembershipId == bungieProfile.PrimaryMembershipId).MembershipType;
                }
                alertText = "found";

                List<DestinyComponentType> Components = new() { DestinyComponentType.Characters };

                PrimaryCharacterProfile = await bungieService.GetProfile(primaryMembershipId, primaryMembershipType, Components);
                if (PrimaryCharacterProfile == null)
                {
                    CharacterErrorAlert.Show();
                }
                DiscordUserInfo = await userManager.SyncBungieProfile(DiscordUserInfo.DiscordUserId, bungieProfile, PrimaryCharacterProfile);
            }


        }

        public async Task ClearBungieProfile()
        {
            DiscordUserInfo = await userManager.ClearBungieProfile(DiscordUserInfo.DiscordUserId);
            bungieProfile = null;
            PrimaryCharacterProfile = null;

            AccountErrorAlert.Hide();
            CharacterErrorAlert.Hide();
            UnlinkAlert.Show();
        }
    }
}
