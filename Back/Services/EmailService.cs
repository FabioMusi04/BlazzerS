using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ReportingPortalServer.Services
{
    public class EmailSettings
    {
        public string FromEmail { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string SmtpServer { get; set; } = default!;
        public int Port { get; set; }
    }

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService(IOptions<EmailSettings> options) : IEmailService
    {
        private readonly EmailSettings _settings = options.Value;

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            using SmtpClient client = new(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.FromEmail, _settings.Password),
                EnableSsl = true
            };

            using MailMessage mail = new(_settings.FromEmail, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail).ConfigureAwait(false);

            return true;
        }
    }
}
