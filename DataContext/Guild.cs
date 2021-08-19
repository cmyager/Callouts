using System;
using System.ComponentModel.DataAnnotations;

namespace Callouts.DataContext
{
    public class Guild
    {
        [Key]
        public ulong GuildId { get; set; }

        // TODO: Add ability to change server prefix
        public string Prefix { get; set; } = "!";
        public Boolean ClearSpam { get; set; } = true;

        // TODO: Figure out if these are needed
        public ulong? EventRoleId { get; set; }
        public ulong? EventDeleteRoleId { get; set; }
    }
}
