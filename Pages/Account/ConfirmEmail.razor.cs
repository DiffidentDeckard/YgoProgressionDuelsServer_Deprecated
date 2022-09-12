using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages.Account
{
    [AllowAnonymous]
    public partial class ConfirmEmail : ComponentBase
    {
        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        private string _statusMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Uri uri = NavManager.ToAbsoluteUri(NavManager.Uri);
            Dictionary<string, StringValues> queryStrings = QueryHelpers.ParseQuery(uri.Query);
            bool succeeded = false;

            try
            {
                // Get the params from the query
                string userIdParam = queryStrings[User.ID].First();
                string confirmationTokenParam = queryStrings[User.EMAIL_CONFIRMATION_TOKEN].First();

                // Parse the confirmation token
                string confirmationToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(confirmationTokenParam.Trim()));

                // Attempt to confirm the email
                ApplicationUser user = await UserManager.FindByIdAsync(userIdParam.Trim());
                IdentityResult result = await UserManager.ConfirmEmailAsync(user, confirmationToken);
                succeeded = result.Succeeded;
            }
            catch
            {
                succeeded = false;
            }

            _statusMessage = succeeded ? "Thank you for confirming your email." :
                "There was an error confirming your email, try sending a new confirmation email from the login screen.";
        }
    }
}
