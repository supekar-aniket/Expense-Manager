using Microsoft.AspNetCore.Identity.UI.Services;

namespace ExpenseManager.Helper
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailHelper _emailHelper;

        public EmailSender(IConfiguration config)
        {
            _emailHelper = new EmailHelper(config);
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _emailHelper.SendEmail(email, subject, htmlMessage);

            return Task.CompletedTask;
        }
    }
}
