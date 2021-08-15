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
        // might need to override OnModelCreating
        // Need to add dbsets
    }
}
