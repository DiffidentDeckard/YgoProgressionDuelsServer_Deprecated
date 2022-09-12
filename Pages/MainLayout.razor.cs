using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationManager NavManager { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; }

        [BindProperty]
        private ApplicationUser _currentUser { get; set; }

        [BindProperty]
        private Theme _theme { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Get the current user
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                _currentUser = await UserManager.FindByNameAsync(authState.User.Identity.Name);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                // Read selected theme from Cookie
                string theme = await JSRuntime.InvokeAsync<string>("ReadCookie.ReadCookie", "theme");

                if (!string.IsNullOrWhiteSpace(theme))
                {
                    _theme = Enum.Parse<Theme>(theme, true);
                    StateHasChanged();
                }
            }
        }

        private async Task OnSelectedThemeChanged(Theme theme)
        {
            if (_theme != theme)
            {
                _theme = theme;
                await JSRuntime.InvokeAsync<object>("WriteCookie.WriteCookie", "theme", theme.ToString(), 365);
                NavManager.NavigateTo(NavManager.Uri, true);
            }
        }
    }
}
