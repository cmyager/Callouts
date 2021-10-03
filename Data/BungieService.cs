using BungieSharper.Client;
using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Config;
using BungieSharper.Entities.Destiny.HistoricalStats;
using BungieSharper.Entities.Destiny.HistoricalStats.Definitions;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BungieSharper.Entities.Destiny.Entities.Characters;

namespace Callouts.Data
{
    public class BungieService
    {
        private readonly BungieApiClient Client;
        //private DestinyManifest ManifestData;

        public BungieService(BungieClientConfig cfg)
        {
            Client = new BungieApiClient(cfg);

            // Could have a periodic to update the bungie.net manifest
            //_ = UpdateManifest();
        }
        //public async Task UpdateManifest()
        //{
        //    ManifestData = await Client.Api.Destiny2_GetDestinyManifest();
        //}

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

        public async Task<UserSearchResponse> SearchByGlobalNamePrefix(string globalname, int page=0)
        {
            UserSearchResponse searchData = new();
            try
            {
                // remove
                searchData = await Client.Api.User_SearchByGlobalNamePrefix(globalname, page);
            }
            catch (Exception) { }
            return searchData;
        }

        public async Task<long?> GetBungieNetIdByBungieName(string bungieName, int bungieCode)
        {
            // remove
            long? retval = null;
            try
            {
                UserSearchResponse searchRetval = await SearchByGlobalNamePrefix(bungieName);
                if (searchRetval.SearchResults != null)
                {
                    // User_SearchByGlobalNamePrefix doesn't accept the code so search for it here
                    UserSearchResponseDetail userData = searchRetval.SearchResults
                        .Where(p => p.BungieGlobalDisplayNameCode == bungieCode).FirstOrDefault();
                    if (userData != null)
                    {
                        retval = userData.BungieNetMembershipId;
                    }
                }
            }
            catch (Exception) { }
            return retval;
        }

        public async Task<IEnumerable<UserInfoCard>> Destiny2_SearchDestinyPlayer(string uniqueName,
            BungieMembershipType membershipType=BungieMembershipType.All)
        {
            IEnumerable<UserInfoCard> retval = null;
            try
            {
                retval = await Client.Api.Destiny2_SearchDestinyPlayer(uniqueName, membershipType);
            }
            catch (Exception) { }
            return retval;
        }

        public async Task<UserInfoCard> GetPrimaryDestinyAccountFromUniqueName(string uniqueName,
            BungieMembershipType membershipType=BungieMembershipType.All)
        {
            UserInfoCard retval = null;
            try
            {
                IEnumerable<UserInfoCard> platforms = await Destiny2_SearchDestinyPlayer(uniqueName, membershipType);
                if (platforms == null || !platforms.Any())
                {

                }
                if (platforms.Count() == 1)
                {
                    retval = platforms.First();
                }
                else
                {
                    retval = platforms.Where(p => p.CrossSaveOverride == p.MembershipType).FirstOrDefault();
                }

            }
            catch (Exception) { }

            return retval;
        }

        public async Task<DestinyProfileResponse> GetProfile(long id, BungieMembershipType membershipType,
            IEnumerable<DestinyComponentType> components = null)
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

        /// <summary>
        /// GetHistoricalStats
        /// </summary>
        /// <param name="destinyMembershipId"></param>
        /// <param name="membershipType"></param>
        /// <param name="modes"></param>
        /// <param name="characterId"></param>
        /// <param name="dayend"></param>
        /// <param name="daystart"></param>
        /// <param name="groups"></param>
        /// <param name="periodType"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, DestinyHistoricalStatsByPeriod>> GetHistoricalStats(long destinyMembershipId, BungieMembershipType membershipType, IEnumerable<DestinyActivityModeType>? modes,
            long characterId = 0, DateTime? dayend = null, DateTime? daystart = null, IEnumerable<DestinyStatsGroupType>? groups = null, PeriodType? periodType = null)
        {
            Dictionary<string, DestinyHistoricalStatsByPeriod> stats = null;

            var groupType = new List<DestinyStatsGroupType> { DestinyStatsGroupType.General };
            try
            {
                stats = await Client.Api.Destiny2_GetHistoricalStats(characterId, destinyMembershipId, membershipType, dayend, daystart, groups, modes, periodType);
            }
            catch (NonRetryErrorCodeException) {}
            return stats;
        }

        /// <summary>
        /// GetActivityHistory
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="destinyMembershipId"></param>
        /// <param name="membershipType"></param>
        /// <param name="count"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<DestinyActivityHistoryResults> GetActivityHistory(DestinyCharacterComponent character, DestinyActivityModeType? mode = null, int? count = null)
        {
            DestinyActivityHistoryResults activity = null;
            try
            {
                activity = await Client.Api.Destiny2_GetActivityHistory(character.CharacterId, character.MembershipId, character.MembershipType, count, mode);
                if (activity.Activities == null || !activity.Activities.Any())
                {
                    activity = null;
                }
            }
            catch (Exception) { }
            return activity;
        }

        /// <summary>
        /// GetPostGameCarnageReport
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public async Task<DestinyPostGameCarnageReportData> GetPostGameCarnageReport(long activityId)
        {
            DestinyPostGameCarnageReportData report = null;
            try
            {
                report = await Client.Api.Destiny2_GetPostGameCarnageReport(activityId);
            }
            catch (Exception) { }
            return report;
        }

        // Might need to store user and type in these to make then useful for the web?
        public class PvPStats
        {
            public string TimePlayed = "-";
            public string Kdr = "-";
            public string Kda = "-";
            public string BestWeapon = "-";
            public string GamesPlayed = "-";
            public string BestSingleGameKills = "-";
            public string LongestSpree = "-";
            public string CombatRating = "-";
            public string Kills = "-";
            public string Assists = "-";
            public string Deaths = "-";
            public string WinRate = "-";
            public string AverageLifeSpan = "-";
            public string LongestLife = "-";
            public string LongestKillDistance = "-";
            public string ActivitiesWon = "-";

            private Dictionary<string, DestinyHistoricalStatsValue> stats;

            public PvPStats(Dictionary<string, DestinyHistoricalStatsValue> pvp_stats)
            {
                stats = pvp_stats;
                BuildStats();
            }
            private void BuildStats()
            {
                if (stats != null)
                {
                    TimePlayed = stats["secondsPlayed"].Basic.DisplayValue;
                    Kdr = stats["killsDeathsRatio"].Basic.DisplayValue;
                    Kda = stats["killsDeathsAssists"].Basic.DisplayValue;
                    BestWeapon = stats["weaponBestType"].Basic.DisplayValue;
                    GamesPlayed = stats["activitiesEntered"].Basic.DisplayValue;
                    BestSingleGameKills = stats["bestSingleGameKills"].Basic.DisplayValue;
                    LongestSpree = stats["longestKillSpree"].Basic.DisplayValue;
                    CombatRating = stats["combatRating"].Basic.DisplayValue;
                    Kills = stats["kills"].Basic.DisplayValue;
                    Assists = stats["assists"].Basic.DisplayValue;
                    Deaths = stats["deaths"].Basic.DisplayValue;
                    AverageLifeSpan = stats["averageLifespan"].Basic.DisplayValue;
                    LongestLife = stats["longestSingleLife"].Basic.DisplayValue;
                    LongestKillDistance = stats["longestKillDistance"].Basic.DisplayValue;
                    ActivitiesWon = stats["activitiesWon"].Basic.DisplayValue;
                    WinRate = $"{CalculateWinRate()}%";
                }
            }
            private double CalculateWinRate()
            {
                double winLossRatio = stats["winLossRatio"].Basic.Value;
                return Math.Round((winLossRatio / (winLossRatio + 1)) * 100, 1);
            }
        }

        public class PvEStats
        {
            // All PvE Stats
            public string TimePlayed = "-";
            public string BestWeapon = "-";
            public string Kills = "-";
            public string Assists = "-";
            public string Deaths = "-";
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

            private Dictionary<string, DestinyHistoricalStatsByPeriod> stats;

            public PvEStats(Dictionary<string, DestinyHistoricalStatsByPeriod> pve_stats)
            {
                stats = pve_stats;
                BuildStats();
            }
            private void BuildStats()
            {
                var allPve = stats.GetValueOrDefault("allPvE");
                if (allPve != null)
                {
                    TimePlayed = allPve.AllTime["totalActivityDurationSeconds"].Basic.DisplayValue;
                    BestWeapon = allPve.AllTime["weaponBestType"].Basic.DisplayValue;
                    Kills = allPve.AllTime["kills"].Basic.DisplayValue;
                    Assists = allPve.AllTime["assists"].Basic.DisplayValue;
                    Deaths = allPve.AllTime["deaths"].Basic.DisplayValue;
                    AverageLifeSpan = allPve.AllTime["averageLifespan"].Basic.DisplayValue;
                    BestSingleGameKills = allPve.AllTime["bestSingleGameKills"].Basic.DisplayValue;
                    OpponentsDefeated = allPve.AllTime["opponentsDefeated"].Basic.DisplayValue;
                    LongestSpree = allPve.AllTime["longestKillSpree"].Basic.DisplayValue;
                    LongestLife = allPve.AllTime["longestSingleLife"].Basic.DisplayValue;
                    LongestKillDistance = allPve.AllTime["longestKillDistance"].Basic.DisplayValue;
                    EventCount = allPve.AllTime["publicEventsCompleted"].Basic.DisplayValue;
                    HeroicEventCount = allPve.AllTime["heroicPublicEventsCompleted"].Basic.DisplayValue;
                }

                var allStrikes = stats.GetValueOrDefault("allStrikes");
                if (allStrikes != null)
                {
                    StrikeCount = allStrikes.AllTime["activitiesCleared"].Basic.DisplayValue;
                }

                var allRaids = stats.GetValueOrDefault("raid");
                if (allRaids != null)
                {
                    RaidCount = allRaids.AllTime["activitiesCleared"].Basic.DisplayValue;
                    RaidTime = allRaids.AllTime["totalActivityDurationSeconds"].Basic.DisplayValue;
                }

                double nightfallCount = 0;
                double fastestNightfallRunValue = -1;

                foreach (string nightfallType in new List<string> { "nightfall", "heroicNightfall", "scored_nightfall", "scored_heroicNightfall" })
                {
                    var nightfallStats = stats.GetValueOrDefault(nightfallType);
                    if (nightfallStats != null && nightfallStats.AllTime != null)
                    {
                        // Find the fastest nightfall run
                        var fastestRunStats = nightfallStats.AllTime["fastestCompletionMs"].Basic;
                        if (fastestNightfallRunValue == -1 || fastestRunStats.Value < fastestNightfallRunValue)
                        {
                            fastestNightfallRunValue = fastestRunStats.Value;
                            FastestNightfall = fastestRunStats.DisplayValue;
                        }
                        // Add total runs of this type to count
                        nightfallCount += nightfallStats.AllTime["activitiesCleared"].Basic.Value;
                    }
                }
                NightFallCount = $"{nightfallCount}";
            }
        }
    }
}