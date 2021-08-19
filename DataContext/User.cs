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
        public ulong UserId { get; set; }
        public long? BungieId { get; set; }
        public long? SteamId { get; set; }
        public long? XboxId { get; set; }
        public long? PsnId { get; set; }
        public long? StadiaId { get; set; }
        public string? BungieName { get; set; }
        public string? SteamName { get; set; }
        public string? XboxName { get; set; }
        public string? PsnName { get; set; }
        public string? StadiaName { get; set; }
        public BungieMembershipType Platform { get; set; }

        public void UnlinkBungieAccount()
        {
            this.BungieId = null;
            this.SteamId = null;
            this.XboxId = null;
            this.PsnId = null;
            this.StadiaId = null;
            this.BungieName = null;
            this.SteamName = null;
            this.XboxName = null;
            this.PsnName = null;
            this.StadiaName = null;
            this.Platform = BungieMembershipType.None;
        }
    }
}
