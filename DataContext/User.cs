using BungieSharper.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

#nullable disable

namespace Callouts.DataContext
{
    public partial class User
    {
        public User()
        {
            Events = new HashSet<Event>();
            UserEvents = new HashSet<UserEvent>();
        }
        [Key]
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("bungie_id")]
        public long? BungieId { get; set; }

        [Column("bungie_name")]
        public string? BungieName { get; set; }

        [Column("platform_id")]
        public long? PrimaryPlatformId { get; set; }

        [Column("platform_name")]
        public string? PrimaryPlatformName { get; set; }

        [Column("platform_type")]
        public BungieMembershipType Platform { get; set; }

        [InverseProperty(nameof(Event.User))]
        public virtual ICollection<Event> Events { get; set; }

        [InverseProperty(nameof(UserEvent.User))]
        public virtual ICollection<UserEvent> UserEvents { get; set; }

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
