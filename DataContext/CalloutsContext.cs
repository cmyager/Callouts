using Microsoft.EntityFrameworkCore;

namespace Callouts.DataContext
{
    public class CalloutsContext : DbContext
    {
        public CalloutsContext(DbContextOptions<CalloutsContext> options)
            : base(options)
        {
        }

    // TODO: Need to add dbsets
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<User> Users { get; set; }
    }
}
