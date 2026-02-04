using System.Net;
using System.Net.Mail;

namespace ExpenseManager.Helper
{
    public class EmailHelper
    {
        private readonly IConfiguration _config;

        public EmailHelper(IConfiguration configuration)
        {
            _config = configuration;
        }

        public bool SendEmail(string? email, string? subject, string? body)
        {
            // Creating objects
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            // Adding information to MailMessage object
            message.From = new MailAddress(_config["EmailSettings:FromEmail"]);
            message.To.Add(email);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;

            // Adding SMTP object information
            smtpClient.Port = 587;
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(
                            _config["EmailSettings:FromEmail"],
                            _config["EmailSettings:AppPassword"]
                        );
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            // Sending Email
            try
            {
                smtpClient.Send(message);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
