using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BungieSharper.Client;
using BungieSharper.Entities;
using BungieSharper.Entities.User;
using BungieSharper.Entities.Destiny.Responses;
using System.Threading.Tasks;
using BungieSharper.Client;
using BungieSharper.Entities;
using BungieSharper.Entities.User;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Config;
using BungieSharper.Entities.Exceptions;
using BungieSharper.Entities.Destiny.Responses;

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
