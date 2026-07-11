using ECommerce.Models;
using System;
using System.Threading.Tasks;

namespace ECommerce1.Services.Payment
{
    public class MomoPaymentProvider : IPaymentProvider
    {
        public string ProviderName => "momo";

        public Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
        {
            // Trả về một Session ID giả lập cho MoMo
            var mockSessionId = "momo_session_" + Guid.NewGuid().ToString().Substring(0, 8);
            return Task.FromResult(mockSessionId);
        }

        public Task<PaymentVerificationResult> VerifySessionAsync(string sessionId)
        {
            // Giả lập thanh toán MoMo luôn thành công
            return Task.FromResult(new PaymentVerificationResult
            {
                IsSuccess = true,
                TransactionId = "momo_trans_" + Guid.NewGuid().ToString().Substring(0, 8),
                Message = "Thanh toán qua ví điện tử MoMo giả lập thành công!"
            });
        }
    }
}
