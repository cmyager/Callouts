using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.UserEvents)
                    .HasForeignKey(d => new { d.EventId })
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.ApplyUtcDateTimeConverter();

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Guild> Guilds { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserEvent> UserEvents { get; set; }
    }

    // Cool thing that sets all datetimes to UTC
    // https://github.com/dotnet/efcore/issues/4711#issuecomment-589842988
    public static class UtcDateAnnotation
    {
        private const string IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
            new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, bool isUtc = true) =>
            builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static bool IsUtc(this IMutableProperty property) =>
            ((bool?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!property.IsUtc())
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime) ||
                        property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcConverter);
                    }
                }
            }
        }
    }
}
