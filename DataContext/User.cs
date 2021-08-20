using BungieSharper.Entities;
using System.ComponentModel.DataAnnotations;

namespace Callouts.DataContext
{
    public class User
    {
        [Key]
        public ulong DiscordUserId { get; set; }
        public long? BungieId { get; set; }
        public string? BungieName { get; set; }
        public long? PrimaryPlatformId { get; set; }
        public string? PrimaryPlatformName { get; set; }
        public BungieMembershipType Platform { get; set; }

        public void UnlinkBungieAccount()
        {
            this.BungieId = null;
            this.BungieName = null;
            this.PrimaryPlatformId = null;
            this.PrimaryPlatformName = null;
            this.Platform = BungieMembershipType.None;
        }
    }
}
