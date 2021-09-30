using BungieSharper.Entities;
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

//TODO: Rename Register?
namespace Callouts.Pages
{
    [Authorize]
    public partial class Bungie : ComponentBase
    {
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        UserManager UserManager { get; set; }

        [Inject]
        BungieService BungieService { get; set; }

        [Inject]
        AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [Inject]
        UserService UserService { get; set; }

        private User DiscordUserInfo = null;

        //TODO: Remove my ID. Makes debugging easier for now
        private string UserSubmitBungieDisplayName { get; set; } = "cmyager";
        private string UserSubmitBungieDisplayNameCode { get; set; } = "8267";
        private UserMembershipData BungieProfile { get; set; }
        private DestinyProfileResponse PrimaryCharacterProfile { get; set; }

        Alert AccountErrorAlert;
        Alert UnlinkAlert;
        Alert CharacterErrorAlert;

        /// <summary>
        /// OnInitializedAsync
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var discUserClaim = UserService.GetInfo(authState);
            if (discUserClaim != null)
            {
                DiscordUserInfo = await UserManager.GetUserByUserId(discUserClaim.UserId);
                if (DiscordUserInfo.BungieId != null)
                {
                    BungieProfile = await BungieService.GetUserById(DiscordUserInfo.BungieId.Value, BungieMembershipType.BungieNext);
                    await GetBungieProfile();
                }
            }
        }

        /// <summary>
        /// GetBungieProfile
        /// </summary>
        /// <returns></returns>
        public async Task GetBungieProfile()
        {
            AccountErrorAlert.Hide();
            UnlinkAlert.Hide();
            CharacterErrorAlert.Hide();

            if (BungieProfile == null && int.TryParse(UserSubmitBungieDisplayNameCode, out int userSubmitBungieDisplayNameCodeInt))
            {
                long? bungieNetId = await BungieService.GetBungieNetIdByBungieName(UserSubmitBungieDisplayName, userSubmitBungieDisplayNameCodeInt);
                if (bungieNetId != null)
                {
                    BungieProfile = await BungieService.GetUserById(bungieNetId.Value, BungieMembershipType.BungieNext);
                }
            }

            if (BungieProfile == null)
            {
                AccountErrorAlert.Show();
            }
            else if (BungieProfile.DestinyMemberships == null || BungieProfile.DestinyMemberships.Count() == 0)
            {
                CharacterErrorAlert.Show();
            }
            else
            {
                long primaryMembershipId = BungieProfile.PrimaryMembershipId.GetValueOrDefault();
                BungieMembershipType primaryMembershipType;

                if (BungieProfile.PrimaryMembershipId == null)
                {
                    primaryMembershipType = BungieProfile.DestinyMemberships.First().MembershipType;
                }
                else
                {
                    primaryMembershipType = BungieProfile.DestinyMemberships.First(p => p.MembershipId == BungieProfile.PrimaryMembershipId).MembershipType;
                }

                List<DestinyComponentType> Components = new() { DestinyComponentType.Characters };

                PrimaryCharacterProfile = await BungieService.GetProfile(primaryMembershipId, primaryMembershipType, Components);
                if (PrimaryCharacterProfile == null)
                {
                    CharacterErrorAlert.Show();
                }
                DiscordUserInfo = await UserManager.SyncBungieProfile(DiscordUserInfo.UserId, BungieProfile, PrimaryCharacterProfile);
            }
        }

        public async Task ClearBungieProfile()
        {
            DiscordUserInfo = await UserManager.ClearBungieProfile(DiscordUserInfo.UserId);
            BungieProfile = null;
            PrimaryCharacterProfile = null;

            AccountErrorAlert.Hide();
            CharacterErrorAlert.Hide();
            UnlinkAlert.Show();
        }
    }
}
