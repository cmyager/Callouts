using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callouts.DataContext
{
    public class CalloutsContext : DbContext
    {
        public CalloutsContext(DbContextOptions<CalloutsContext> options)
            : base(options)
        {
        }

    // Need to add dbsets
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<User> Users { get; set; }
    }
}
