// ==========================================================================
// MODULE: IEmailService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module IEmailService
// ==========================================================================
using ECommerce.Models;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendOrderStatusEmailAsync(Order order, string statusType, string? customNote = null, int failedCount = 1);
    }
}
