using ECommerce.Models;
using System.Threading.Tasks;

namespace ECommerce1.Services.Payment
{
    public interface IPaymentProvider
    {
        string ProviderName { get; }
        Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl);
        Task<PaymentVerificationResult> VerifySessionAsync(string sessionId);
    }
    
    public class PaymentVerificationResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}
