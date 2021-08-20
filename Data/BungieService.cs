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
            catch (NonRetryErrorCodeException) {}
            return profile;
        }

        public async Task<DestinyLinkedProfilesResponse> GetLinkedProfiles(long id)
        {
            DestinyLinkedProfilesResponse profile = null;
            try
            {
                profile = await Client.Api.Destiny2_GetLinkedProfiles(id, BungieMembershipType.All);
            }
            catch (NonRetryErrorCodeException) {}
            return profile;
        }

        //public async Task<Dictionary<string, DestinyHistoricalStatsByPeriod>> GetHistoricalStats(long characterId, long destinyMembershipId, BungieMembershipType membershipType, DateTime? dayend = null,
        //    DateTime? daystart = null, IEnumerable<DestinyStatsGroupType>? groups = null, IEnumerable<DestinyActivityModeType>? modes = null, PeriodType? periodType = null)
        public async Task<Dictionary<string, DestinyHistoricalStatsByPeriod>> GetHistoricalStats(long destinyMembershipId, BungieMembershipType membershipType, IEnumerable<DestinyActivityModeType> modes)
        {
            Dictionary<string, DestinyHistoricalStatsByPeriod> stats = null;

            long allCharactersId = 0;
            var groupType = new List<DestinyStatsGroupType> { DestinyStatsGroupType.General };
            try
            {
                stats = await Client.Api.Destiny2_GetHistoricalStats(allCharactersId, destinyMembershipId, membershipType, null, null, groupType, modes, null);
            }
            catch (NonRetryErrorCodeException) {}
            return stats;
        }

        public class PvPStats
        {
            public PvPStats(Dictionary<string, DestinyHistoricalStatsByPeriod> pvp_stats)
            {
                var x = 5;
            }
        }

        public class PvEStats
        {
            // All PvE Stats
            public string TimePlayed = "-";
            public string BestWeapon = "-";
            public string kills = "-";
            public string assists = "-";
            public string deaths = "-";
            public string AverageLifeSpan = "-";
            public string BestSingleGameKills = "-";
            public string OpponentsDefeated = "-";
            public string LongestSpree = "-";
            public string LongestLife = "-";
            public string LongestKillDistance = "-";
            public string EventCount = "-";
            public string HeroicEventCount = "-";

            // Strike Stats
            public string StrikeCount = "-";

            // Raid Stats
            public string RaidCount = "-";
            public string RaidTime = "-";

            // Nightfall Stats
            public string NightFallCount = "-";
            public string FastestNightfall = "-";

            public PvEStats(Dictionary<string, DestinyHistoricalStatsByPeriod> pve_stats)
            {
                var allPve = pve_stats.GetValueOrDefault("allPvE");
                if (allPve != null)
                {
                    TimePlayed = allPve.AllTime["totalActivityDurationSeconds"].Basic.DisplayValue;
                    BestWeapon = allPve.AllTime["weaponBestType"].Basic.DisplayValue;
                    kills = allPve.AllTime["kills"].Basic.DisplayValue;
                    assists = allPve.AllTime["assists"].Basic.DisplayValue;
                    deaths = allPve.AllTime["deaths"].Basic.DisplayValue;
                    AverageLifeSpan = allPve.AllTime["averageLifespan"].Basic.DisplayValue;
                    BestSingleGameKills = allPve.AllTime["bestSingleGameKills"].Basic.DisplayValue;
                    OpponentsDefeated = allPve.AllTime["opponentsDefeated"].Basic.DisplayValue;
                    LongestSpree = allPve.AllTime["longestKillSpree"].Basic.DisplayValue;
                    LongestLife = allPve.AllTime["longestSingleLife"].Basic.DisplayValue;
                    LongestKillDistance = allPve.AllTime["longestKillDistance"].Basic.DisplayValue;
                    EventCount = allPve.AllTime["publicEventsCompleted"].Basic.DisplayValue;
                    HeroicEventCount = allPve.AllTime["heroicPublicEventsCompleted"].Basic.DisplayValue;
                }

                var allStrikes = pve_stats.GetValueOrDefault("allStrikes");
                if (allStrikes != null)
                {
                    StrikeCount = allStrikes.AllTime["activitiesCleared"].Basic.DisplayValue;
                }

                var allRaids = pve_stats.GetValueOrDefault("raid");
                if (allRaids != null)
                {
                    RaidCount = allRaids.AllTime["activitiesCleared"].Basic.DisplayValue;
                    RaidTime = allRaids.AllTime["totalActivityDurationSeconds"].Basic.DisplayValue;
                }

                // TODO NIGHTFALLS
            }
        }
    }
}