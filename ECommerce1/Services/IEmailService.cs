using ECommerce.Models;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendOrderStatusEmailAsync(Order order, string statusType, string? customNote = null, int failedCount = 1);
    }
}
