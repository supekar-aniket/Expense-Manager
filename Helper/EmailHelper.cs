using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ExpenseManager.Helper
{
    public class EmailHelper
    {
        private readonly IConfiguration _config;

        public EmailHelper(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task<bool> SendEmailAsync(string? email, string? subject, string? body)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["EmailSettings:FromEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(
                    _config["EmailSettings:FromEmail"],
                    _config["EmailSettings:AppPassword"]
                );

                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
