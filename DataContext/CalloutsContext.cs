using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

// TODO: Clean up
// TODO: More complex replationships that do on delete cascade
// TODO: all of the fields that should be with the foreign IDs are null. Dunno why. EF is hard

#nullable disable

namespace Callouts.DataContext
{
    public partial class CalloutsContext : DbContext
    {
        public CalloutsContext(DbContextOptions<CalloutsContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<Guild>(entity =>
            {
                entity.HasKey(e => new { e.GuildId })
                    .HasName("Primary");
                entity.Property(e => e.GuildId)
                    .ValueGeneratedNever();

                //entity.Property(e => e.ClearSpam).HasDefaultValueSql("1");
                //entity.Property(e => e.Prefix).HasDefaultValueSql("!");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => new { e.UserId })
                    .HasName("Primary");
                entity.Property(e => e.UserId)
                    .ValueGeneratedNever();

            });


            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => new { e.EventId })
                    .HasName("PRIMARY");
                entity.Property(e => e.StartTime)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.EventId)
                        .ValueGeneratedOnAdd();

                entity.HasOne(d => d.Guild)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.GuildId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserEvent>(entity =>
            {
                    entity.HasKey(e => new { e.UserEventId })
                        .HasName("PRIMARY");
                //    entity.HasKey(e => new { e.UserEventId, e.UserId, e.GuildId, e.Title })
                //        .HasName("PRIMARY");

                //entity.HasOne(d => d.User)
                //    .WithMany(p => p.UserEvents)
                //    .HasForeignKey(d => d.UserId)
                //    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.UserEvents)
                    .HasForeignKey(d => new { d.EventId })
                    .OnDelete(DeleteBehavior.Cascade);

                //entity.HasOne(d => d.Guild)
                //    .WithMany(d => d.UserEvents)
                //    .HasForeignKey(d => d.GuildId)
                //    .OnDelete(DeleteBehavior.Cascade);






                //entity.Property(e => e.Attempts).HasDefaultValueSql("1");
                //entity.Property(e => e.Confirmed).HasDefaultValueSql("0");
                //entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            //////////////////////////////////////////////////


            //modelBuilder.Entity<UserEvent>(entity =>
            //{
            //    entity.HasKey(e => new { e.UserEventId, e.UserId, e.GuildId, e.Title })
            //        .HasName("PRIMARY");

            //    entity.HasOne(d => d.User)
            //        .WithMany(p => p.UserEvents)
            //        .HasForeignKey(d => d.UserId);

            //    entity.HasOne(d => d.Event)
            //        .WithMany(p => p.UserEvents)
            //        .HasForeignKey(d => new { d.EventId, d.GuildId, d.Title });
            //});

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        // TODO: Need to add dbsets
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Guild> Guilds { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserEvent> UserEvents { get; set; }
    }
}
