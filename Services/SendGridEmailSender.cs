using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Services
{
    /// <summary>
    /// Class to send emails using SendGrid
    /// Gets injected in startup.cs
    /// </summary>
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridSettings _sendGridSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public SendGridEmailSender(IOptions<SendGridSettings> sendGridSettings, UserManager<ApplicationUser> userManager)
        {
            _sendGridSettings = sendGridSettings.Value;
            _userManager = userManager;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ApplicationUser recipientUser = await _userManager.FindByEmailAsync(email);
            if (recipientUser == null)
            {
                // No user with this email exists in our database, ignore this request
                // TODO: Show an error toaster message
                return;
            }

            EmailAddress sender = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromUserName);
            EmailAddress recipient = new EmailAddress(recipientUser.Email, recipientUser.UserName);

            SendGridMessage message = MailHelper.CreateSingleEmail(sender, recipient, $"{Application.NAME}: {subject}", string.Empty, $"Hi {recipientUser},<br/>{htmlMessage}");

            SendGridClient emailClient = new SendGridClient(_sendGridSettings.ApiKey);
            Response response = await emailClient.SendEmailAsync(message);

            if (response.IsSuccessStatusCode)
            {
                // TODO: Show a success toaster message
            }
            else
            {
                // TODO: Show a failure toaster message
            }
        }
    }

    /// <summary>
    /// Class to represent the appsettings.json values for the "SendGridSettings" node
    /// The properties of this class should have the exact same name as the keys in that node
    /// </summary>
    public class SendGridSettings
    {
        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public string FromEmail { get; set; }
        public string FromUserName { get; set; }
    }
}
