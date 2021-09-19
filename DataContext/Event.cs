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
        public int EventId { get; set; }

        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // TODO: restrict this to positive? or null
        // incase you want to allow creating an event no limit
        [Column("max_members")]
        public int? MaxMembers { get; set; }

        [Required]
        [Column("start_time", TypeName = "datetime")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(GuildId))]
        [InverseProperty("Events")]
        public virtual Guild Guild { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("Events")]
        public virtual User User { get; set; }

        [InverseProperty(nameof(UserEvent.Event))]
        public virtual ICollection<UserEvent> UserEvents { get; set; }

        [NotMapped]
        public DateTime CentralTime => TimeZoneInfo.ConvertTime(this.StartTime, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

        [NotMapped]
        public List<UserEvent> Accepted { get { return GetAccepted(); } }

        [NotMapped]
        public List<UserEvent> Standby { get { return GetStandby(); } }

        // TODO Consider: These could be removed and this could just be done by sorting by last update
        // It would simplify the event creation a bit I imagine, but this makes it readable
        [NotMapped]
        public List<UserEvent> Declined
            => this.UserEvents
            .Where(p => p.Attending == UserEventAttending.DECLINED || p.Attending == UserEventAttending.REJECTED )
            .OrderBy(p => p.LastUpdated)
            .ToList();

        [NotMapped]
        public List<UserEvent> Maybe
            => this.UserEvents.Where(p => p.Attending == UserEventAttending.MAYBE)
            .OrderBy(p => p.LastUpdated)
            .ToList();

        /// <summary>
        /// GetAccepted
        /// </summary>
        /// <returns></returns>
        private List<UserEvent> GetAccepted()
        {
            List<UserEvent> attending = this.UserEvents
                .Where(p => p.Attending == UserEventAttending.ACCEPTED || p.Attending == UserEventAttending.CONFIRMED)
                .OrderBy(p => p.LastUpdated).ToList();

            if (this.MaxMembers != null && attending.Count > this.MaxMembers)
            {
                attending = attending.GetRange(0, this.MaxMembers.Value);
            }
            return attending;
        }

        /// <summary>
        /// GetStandby
        /// </summary>
        /// <returns></returns>
        private List<UserEvent> GetStandby()
        {
            List<UserEvent> standby = new();
            List<UserEvent> accepted = this.UserEvents
                .Where(p => p.Attending == UserEventAttending.ACCEPTED || p.Attending == UserEventAttending.CONFIRMED)
                .OrderBy(p => p.LastUpdated).ToList();

            if (this.MaxMembers != null && accepted.Count > this.MaxMembers)
            {
                standby = accepted.GetRange(this.MaxMembers.Value, accepted.Count - this.MaxMembers.Value);
            }
            return standby;
        }
    }
}
