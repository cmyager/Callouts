using BungieSharper.Client;
using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Config;
using BungieSharper.Entities.Destiny.HistoricalStats;
using BungieSharper.Entities.Destiny.HistoricalStats.Definitions;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Callouts.Data
{
    public class BungieService
    {
        private readonly BungieApiClient Client;
        private DestinyManifest ManifestData;

        public BungieService(BungieClientConfig cfg)
        {
            Client = new BungieApiClient(cfg);
            // This can run in the background
            // TODO: Some sort of periodic to update this
            _ = UpdateManifest();
        }
        public async Task UpdateManifest()
        {
            ManifestData = await Client.Api.Destiny2_GetDestinyManifest();
        }

        public async Task<UserMembershipData> GetUserById(long id, BungieMembershipType membershipType)
        {
            UserMembershipData userData = null;
            try
            {
                userData = await Client.Api.User_GetMembershipDataById(id, membershipType);
            }
            catch (NonRetryErrorCodeException) {}
            return userData;
        }

        public async Task<DestinyProfileResponse> GetProfile(long id, BungieMembershipType membershipType, IEnumerable<DestinyComponentType> components = null)
        {
            DestinyProfileResponse profile = null;
            try
            {
                profile = await Client.Api.Destiny2_GetProfile(id, membershipType, components);
            }
            catch (NonRetryErrorCodeException) { }
            return profile;
        }

        public async Task<DestinyLinkedProfilesResponse> GetLinkedProfiles(long id)
        {
            DestinyLinkedProfilesResponse profile = null;
            try
            {
                profile = await Client.Api.Destiny2_GetLinkedProfiles(id, BungieMembershipType.All);
            }
            catch (NonRetryErrorCodeException) { }
            return profile;
        }
    }
}