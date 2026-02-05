using Microsoft.AspNetCore.Identity.UI.Services;

namespace ExpenseManager.Helper
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailHelper _emailHelper;

        public EmailSender(EmailHelper emailHelper)
        {
            _emailHelper = emailHelper;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _emailHelper.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
