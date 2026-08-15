// ==========================================================================
// MODULE: EmailService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module EmailService
// ==========================================================================
using ECommerce.Models;
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

            // Trong môi trường dev, nếu chưa cấu hình EmailSettings hoặc vẫn là placeholder,thì log ra Console
            if (string.IsNullOrEmpty(host) || 
                string.IsNullOrEmpty(username) || 
                username == "YOUR_GMAIL_USERNAME" || 
                !username.Contains("@"))
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
                        BodyEncoding = System.Text.Encoding.UTF8,
                        SubjectEncoding = System.Text.Encoding.UTF8
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

        public async Task SendOrderStatusEmailAsync(Order order, string statusType, string? customNote = null, int failedCount = 1)
        {
            if (order == null) return;

            string recipientEmail = !string.IsNullOrWhiteSpace(order.ReceiverEmail) ? order.ReceiverEmail : order.User?.Email;
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                _logger.LogWarning($"[EMAIL SKIP] Không thể gửi mail thông báo cho đơn hàng #{order.Id} vì không tìm thấy email người nhận.");
                return;
            }

            string frontendBaseUrl = _configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = "http://localhost:5173";
            }

            string subject = "";
            string htmlMessage = "";

            switch (statusType.ToLower())
            {
                case "placed":
                case "pending":
                    subject = $"[PhoneStore] Xác nhận đơn hàng #{order.Id} thành công";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderPlacedEmailHtml(order, frontendBaseUrl);
                    break;

                case "confirmed":
                case "preparing":
                    subject = $"[PhoneStore] Xác nhận đơn hàng #{order.Id} thành công";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderConfirmedEmailHtml(order, frontendBaseUrl);
                    break;

                case "shipping":
                    subject = $"[PhoneStore] Đơn hàng #{order.Id} đang được giao đến bạn";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderShippingEmailHtml(order, frontendBaseUrl);
                    break;

                case "delivered":
                    subject = $"[PhoneStore] Đơn hàng #{order.Id} đã được giao thành công";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderDeliveredEmailHtml(order, frontendBaseUrl);
                    break;

                case "cancelled":
                    subject = $"[PhoneStore] Thông báo hủy đơn hàng #{order.Id}";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderCancelledEmailHtml(order, frontendBaseUrl, customNote);
                    break;

                case "refunded":
                    subject = $"[PhoneStore] Xác nhận hoàn tiền đơn hàng #{order.Id}";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderRefundedEmailHtml(order, frontendBaseUrl, customNote);
                    break;

                case "shipping_failed":
                    subject = $"[PhoneStore] Thông báo giao hàng chưa thành công đơn #{order.Id}";
                    htmlMessage = OrderEmailTemplateHelper.GetOrderShippingFailedEmailHtml(order, frontendBaseUrl, failedCount);
                    break;

                default:
                    return;
            }

            await SendEmailAsync(recipientEmail, subject, htmlMessage);
        }
    }
}
