using System;
using System.ComponentModel.DataAnnotations;
using BungieSharper.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Callouts.DataContext
{
    public partial class Guild
    {
        public Guild()
        {
            Events = new HashSet<Event>();
            UserEvents = new HashSet<UserEvent>();
        }
        [Key]
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        // Add ability to change server prefix?
        [Column("prefix")]
        public string Prefix { get; set; } = "!";

        [Column("clear_spam")]
        public bool ClearSpam { get; set; } = true;

        // Add event create / delete roles?
        // [Column("event_role_id")]
        // public ulong? EventRoleId { get; set; }

        // [Column("event_delete_role_id")]
        // public ulong? EventDeleteRoleId { get; set; }

        [InverseProperty(nameof(Event.Guild))]
        public virtual ICollection<Event> Events { get; set; }

        [InverseProperty(nameof(UserEvent.Guild))]
        public virtual ICollection<UserEvent> UserEvents { get; set; }
    }
}
