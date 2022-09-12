using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace YgoProgressionDuels.Pages.Account
{
    [AllowAnonymous]
    public partial class Login : ComponentBase
    {
        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        private SignInManager<ApplicationUser> SignInManager { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [BindProperty]
        private LoginUserModel _loginUserModel { get; set; } = new LoginUserModel();

        [BindProperty]
        private string _userLoginError { get; set; }

        public async Task OnLoginUserAsync()
        {
            _userLoginError = null;

            // Get ApplicationUser
            ApplicationUser user = await UserManager.FindByNameAsync(_loginUserModel.UserName);
            if (user == null)
            {
                _userLoginError = "Invalid login attempt";
                return;
            }

            // Check if email is confirmed (if required)
            if (UserManager.Options.SignIn.RequireConfirmedEmail
                && !await UserManager.IsEmailConfirmedAsync(user))
            {
                _userLoginError = "The email for this account hasn't been confirmed.";
            }

            // Check if account can be signed into
            if (!await SignInManager.CanSignInAsync(user))
            {
                _userLoginError = "Account is locked.";
                return;
            }

            // Check that password is correct
            SignInResult result = await SignInManager.CheckPasswordSignInAsync(user, _loginUserModel.Password, false);
            if (result == SignInResult.Success)
            {
                // Sign in through the middleware, because SingInManager.PasswordSIgnInAsync() doesn't work in pure Blazor
                Guid key = Guid.NewGuid();
                BlazorCookieLoginMiddleware.Logins[key] = _loginUserModel;
                NavManager.NavigateTo($"/Account/Login?key={key}", true);
            }
            else
            {
                _userLoginError = "Invalid login attempt";
            }
        }
    }
}
