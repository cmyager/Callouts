using BungieSharper.Entities;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.Data;
using Callouts.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;

namespace Callouts.Pages
{
    public partial class NewEvent : ComponentBase
    {
        protected string eventName;
        protected string eventDiscription;
        
    }
}
