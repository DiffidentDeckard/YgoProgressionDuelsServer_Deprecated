using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Shared;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace YgoProgressionDuels.Pages.Account
{
    [AllowAnonymous]
    public partial class Register : ComponentBase
    {
        [Inject]
        private IEmailSender EmailSender { get; set; }

        [Inject]
        private IWebHostEnvironment WebHostEnvironment { get; set; }

        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        private SignInManager<ApplicationUser> SignInManager { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }


        [BindProperty]
        private RegisterUserModel _registerUserModel { get; set; } = new RegisterUserModel();

        [BindProperty]
        private IList<string> _defaultAvatarUrls { get; set; }

        [BindProperty]
        private IList<string> _userCreationErrors { get; set; } = new List<string>();

        protected override void OnInitialized()
        {
            // Get the default avatar image urls
            _defaultAvatarUrls = Directory.EnumerateFiles(Path.Combine(WebHostEnvironment.WebRootPath, "images", "avatars", "defaults"))
                .Select(filePath => filePath.Replace(WebHostEnvironment.WebRootPath, string.Empty)).ToList();
            _registerUserModel.AvatarUrl = _defaultAvatarUrls[new Random().Next(_defaultAvatarUrls.Count)];
        }

        public async Task OnRegisterUserAsync()
        {
            // Create ApplicationUser
            ApplicationUser user = new ApplicationUser { UserName = _registerUserModel.UserName, Email = _registerUserModel.Email, AvatarUrl = _registerUserModel.AvatarUrl };
            IdentityResult identityResult = await UserManager.CreateAsync(user, _registerUserModel.Password);
            _userCreationErrors = identityResult.Errors.Select(error => error.Description).ToList();

            if (!identityResult.Succeeded)
            {
                return;
            }

            // Generate email confirmation token
            string emailConfirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            emailConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

            // Create URL for user to click to confirm their email
            string callbackUrl = NavManager.ToAbsoluteUri("/Account/ConfirmEmail").AbsoluteUri;
            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, User.ID, user.Id.ToString());
            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, User.EMAIL_CONFIRMATION_TOKEN, emailConfirmationToken);

            // Send confirmation email
            await EmailSender.SendEmailAsync(_registerUserModel.Email, "Please Confirm Your Email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            if (UserManager.Options.SignIn.RequireConfirmedEmail)
            {
                NavManager.NavigateTo("/Account/RegisterConfirmation");
            }
            else
            {
                // If confirmed email is not required, log new user in immediately
                SignInResult signInResult = await SignInManager.CheckPasswordSignInAsync(user, _registerUserModel.Password, false);
                if (signInResult == SignInResult.Success)
                {
                    Guid key = Guid.NewGuid();
                    BlazorCookieLoginMiddleware.Logins[key] = new LoginUserModel() { UserName = _registerUserModel.UserName, Password = _registerUserModel.Password };
                    NavManager.NavigateTo($"/Account/Login?key={key}", true);
                }
                else
                {
                    _userCreationErrors.Add("Invalid login attempt");
                }
            }
        }
    }
}
