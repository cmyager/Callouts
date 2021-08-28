using BungieSharper.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Callouts.DataContext
{
    public partial class Event
    {
        public Event()
        {
            UserEvents = new HashSet<UserEvent>();
        }

        [Key]
        [Column("event_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; private set; }
        //public int? EventId { get; private set; }
        //TODO

        [Column("guild_id")]
        public ulong GuildId { get; set; }

        // TODO: Should this be null?
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // TODO: restrict this to positive? or null
        [Column("max_members")]
        public int? MaxMembers { get; set; }

        [Required]
        [Column("start_time", TypeName = "datetime")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;


        // TODO: Put this in place later
        // [Column("utctime", TypeName = "datetime")]
        // public DateTime Utctime { get; set; }

        // [Column("timezone")]
        // [StringLength(20)]
        // public string Timezone { get; set; }


        [ForeignKey(nameof(GuildId))]
        [InverseProperty("Events")]
        public virtual Guild Guild { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("Events")]
        public virtual User User { get; set; }

        [InverseProperty(nameof(UserEvent.Event))]
        public virtual ICollection<UserEvent> UserEvents { get; set; }

    }
}
