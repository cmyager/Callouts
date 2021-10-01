using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Callouts.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    guild_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    prefix = table.Column<string>(type: "TEXT", nullable: true),
                    clear_spam = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Primary", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    bungie_id = table.Column<long>(type: "INTEGER", nullable: true),
                    bungie_name = table.Column<string>(type: "TEXT", nullable: true),
                    platform_id = table.Column<long>(type: "INTEGER", nullable: true),
                    platform_name = table.Column<string>(type: "TEXT", nullable: true),
                    platform_type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Primary", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guild_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    user_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    max_members = table.Column<int>(type: "INTEGER", nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_Events_Guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "Guilds",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEvents",
                columns: table => new
                {
                    user_event_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    guild_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    event_id = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    attending = table.Column<int>(type: "INTEGER", nullable: false),
                    last_updated = table.Column<DateTime>(type: "datetime", nullable: false),
                    attempts = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.user_event_id);
                    table.ForeignKey(
                        name: "FK_UserEvents_Events_event_id",
                        column: x => x.event_id,
                        principalTable: "Events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEvents_Guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "Guilds",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEvents_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_guild_id",
                table: "Events",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_Events_user_id",
                table: "Events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserEvents_event_id",
                table: "UserEvents",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserEvents_guild_id",
                table: "UserEvents",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserEvents_user_id",
                table: "UserEvents",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEvents");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
