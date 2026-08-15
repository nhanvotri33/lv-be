// ==========================================================================
// MODULE: IPaymentProvider.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module IPaymentProvider
// ==========================================================================
using ECommerce.Models;
using System.Threading.Tasks;

namespace ECommerce1.Services.Payment
{
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IPaymentProvider
    {
        string ProviderName { get; }
        Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl);
        Task<PaymentVerificationResult> VerifySessionAsync(string sessionId);
        Task<bool> RefundAsync(string transactionId, decimal amount);
    }
    
    public class PaymentVerificationResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}
