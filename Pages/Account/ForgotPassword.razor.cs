using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages.Account
{
    public partial class ForgotPassword : ComponentBase
    {
        [Inject]
        private IEmailSender EmailSender { get; set; }

        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }


        [BindProperty]
        private ForgotPasswordModel _forgotPasswordModel { get; set; } = new ForgotPasswordModel();

        public async Task OnPasswordResetAsync()
        {
            ApplicationUser user = await UserManager.FindByEmailAsync(_forgotPasswordModel.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                NavManager.NavigateTo("/Account/ForgotPasswordConfirmation");
            }

            // Generate password reset token
            string passwordResetToken = await UserManager.GeneratePasswordResetTokenAsync(user);
            passwordResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetToken));

            // Create URL for user to click to reset their password
            string callbackUrl = NavManager.ToAbsoluteUri("/Account/ResetPassword").AbsoluteUri;
            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, User.ID, user.Id.ToString());
            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, User.PASSWORD_RESET_TOKEN, passwordResetToken);

            // Send password reset email
            await EmailSender.SendEmailAsync(
                _forgotPasswordModel.Email,
                "You Requested A Password Reset",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            NavManager.NavigateTo("./ForgotPasswordConfirmation");
        }
    }
}
