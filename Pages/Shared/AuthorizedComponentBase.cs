using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Pages.Shared
{
    /// <summary>
    /// A convenient base so I don't have to copy this stuff into every single component
    /// </summary>
    [Authorize]
    public abstract class AuthorizedComponentBase : ComponentBase
    {
        [Inject]
        protected IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [Inject]
        protected NavigationManager NavManager { get; set; }

        [BindProperty]
        protected bool IsLoading { get; set; }

        protected Guid CurrentUserId { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetCurrentUserIdAsync();
            await base.OnInitializedAsync();
        }

        protected async Task<Guid> GetCurrentUserIdAsync()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            return Guid.Parse(authState.User.Claims.First(claim => claim.Type.Equals(ClaimTypes.NameIdentifier)).Value);
        }
    }
}
