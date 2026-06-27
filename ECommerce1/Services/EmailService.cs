using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailConfig = _configuration.GetSection("EmailSettings");
            var host = emailConfig["Host"];
            var portString = emailConfig["Port"];
            var username = emailConfig["Username"];
            var password = emailConfig["Password"];
            var fromEmail = emailConfig["FromEmail"];

            // Trong môi trường dev, nếu chưa cấu hình EmailSettings, ta sẽ log ra Console
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning($"[MOCK EMAIL] To: {toEmail} | Subject: {subject}\n{htmlMessage}");
                return;
            }

            try
            {
                int port = int.Parse(portString);

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail ?? username, "PhoneStore"),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đến {ToEmail}", toEmail);
                throw new Exception("Không thể gửi email. Vui lòng thử lại sau.");
            }
        }
    }
}
