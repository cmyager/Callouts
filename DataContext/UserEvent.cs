using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using BungieSharper.Entities;


// TODO: Clean up
// TODO: More complex replationships that do on delete cascade
#nullable disable
namespace Callouts.DataContext
{
    public enum UserEventAttending
    {
        UNKNOWN = 0,
        DECLINED = 1,
        MAYBE = 2,
        ACCEPTED = 3,
        CONFIRMED = 4,
        REJECTED = 5
    }

    public partial class UserEvent
    {

        [Key]
        [Column("user_event_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? UserEventId { get; set; }
        //public int? UserEventId { get; set; }
        //TODO

        //[Key]
        [Column("user_id")]
        public ulong UserId { get; set; }

        //[Key]
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        //[Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("attending")]
        public UserEventAttending Attending { get; set; }

        [Column("last_updated", TypeName = "datetime")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // [Column("confirmed")]
        // public int Confirmed { get; set; } = 0;

        [Column("attempts")]
        public int Attempts { get; set; } = 1;


        [ForeignKey(nameof(EventId))]
        [InverseProperty("UserEvents")]
        public virtual Event Event { get; set; }

        [ForeignKey(nameof(GuildId))]
        [InverseProperty("UserEvents")]
        public virtual Guild Guild { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("UserEvents")]
        public virtual User User { get; set; }
    }
}