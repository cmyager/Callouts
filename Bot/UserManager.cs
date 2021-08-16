using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Callouts.DataContext;

namespace Callouts
{
    public class UserManager
    {
        private readonly IDbContextFactory<CalloutsContext> ContextFactory;
        public UserManager(IDbContextFactory<CalloutsContext> contextFactory)
        {
            ContextFactory = contextFactory;
        }
    }
}