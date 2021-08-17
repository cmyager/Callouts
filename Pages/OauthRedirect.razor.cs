using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Callouts.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Callouts.Pages
{
    public partial class OauthRedirect : ComponentBase
    {
        [Inject]
        public NavigationManager NavManager { get; set; }

        [Inject]
        internal CustomAuthStateProvider userSession { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out var _code))
            {
                await userSession.AuthFromCode(_code);
                await userSession.CheckAuth();
                NavManager.NavigateTo("/");
            }
            else
            {
            }
        }
    }
}
