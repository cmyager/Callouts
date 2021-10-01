﻿// <auto-generated />
using System;
using Callouts.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Callouts.Migrations
{
    [DbContext(typeof(CalloutsContext))]
    partial class CalloutsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.8");

            modelBuilder.Entity("Callouts.DataContext.Event", b =>
                {
                    b.Property<int>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("event_id");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("guild_id");

                    b.Property<int?>("MaxMembers")
                        .HasColumnType("INTEGER")
                        .HasColumnName("max_members");

                    b.Property<DateTime>("StartTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("start_time")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("title");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("EventId")
                        .HasName("PRIMARY");

                    b.HasIndex("GuildId");

                    b.HasIndex("UserId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Callouts.DataContext.Guild", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("guild_id");

                    b.Property<bool>("ClearSpam")
                        .HasColumnType("INTEGER")
                        .HasColumnName("clear_spam");

                    b.Property<string>("Prefix")
                        .HasColumnType("TEXT")
                        .HasColumnName("prefix");

                    b.HasKey("GuildId")
                        .HasName("Primary");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Callouts.DataContext.User", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.Property<long?>("BungieId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("bungie_id");

                    b.Property<string>("BungieName")
                        .HasColumnType("TEXT")
                        .HasColumnName("bungie_name");

                    b.Property<int>("Platform")
                        .HasColumnType("INTEGER")
                        .HasColumnName("platform_type");

                    b.Property<long?>("PrimaryPlatformId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("platform_id");

                    b.Property<string>("PrimaryPlatformName")
                        .HasColumnType("TEXT")
                        .HasColumnName("platform_name");

                    b.HasKey("UserId")
                        .HasName("Primary");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Callouts.DataContext.UserEvent", b =>
                {
                    b.Property<int?>("UserEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_event_id");

                    b.Property<int>("Attempts")
                        .HasColumnType("INTEGER")
                        .HasColumnName("attempts");

                    b.Property<int>("Attending")
                        .HasColumnType("INTEGER")
                        .HasColumnName("attending");

                    b.Property<int>("EventId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("event_id");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("guild_id");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime")
                        .HasColumnName("last_updated");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT")
                        .HasColumnName("title");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("UserEventId")
                        .HasName("PRIMARY");

                    b.HasIndex("EventId");

                    b.HasIndex("GuildId");

                    b.HasIndex("UserId");

                    b.ToTable("UserEvents");
                });

            modelBuilder.Entity("Callouts.DataContext.Event", b =>
                {
                    b.HasOne("Callouts.DataContext.Guild", "Guild")
                        .WithMany("Events")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Callouts.DataContext.User", "User")
                        .WithMany("Events")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Callouts.DataContext.UserEvent", b =>
                {
                    b.HasOne("Callouts.DataContext.Event", "Event")
                        .WithMany("UserEvents")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Callouts.DataContext.Guild", "Guild")
                        .WithMany("UserEvents")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Callouts.DataContext.User", "User")
                        .WithMany("UserEvents")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Callouts.DataContext.Event", b =>
                {
                    b.Navigation("UserEvents");
                });

            modelBuilder.Entity("Callouts.DataContext.Guild", b =>
                {
                    b.Navigation("Events");

                    b.Navigation("UserEvents");
                });

            modelBuilder.Entity("Callouts.DataContext.User", b =>
                {
                    b.Navigation("Events");

                    b.Navigation("UserEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
