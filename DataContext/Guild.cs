using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Callouts.DataContext
{
    public class Guild
    {
        [Key]
        public ulong GuildId { get; set; }
        public string Prefix { get; set; } = "!";
        public Boolean ClearSpam { get; set; } = true;
        public ulong? EventRoleId { get; set; }
        public ulong? EventDeleteRoleId { get; set; }
    }
}
