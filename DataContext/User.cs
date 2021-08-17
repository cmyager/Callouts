using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Callouts.DataContext
{
    public class User
    {
        [Key]
        public ulong UserId { get; set; }
        public ulong? BungieId { get; set; }
        public ulong? SteamId { get; set; }
        public ulong? XboxId { get; set; }
        public ulong? PsnId { get; set; }
        public ulong? StadiaId { get; set; }
        public string? BungieName { get; set; }
        public string? SteamName { get; set; }
        public string? XboxName { get; set; }
        public string? PsnName { get; set; }
        public string? StadiaName { get; set; }
        public Platform Platform { get; set; }
    }
}
