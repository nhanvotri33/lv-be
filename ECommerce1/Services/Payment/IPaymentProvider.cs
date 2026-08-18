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

        // Kết quả này có thực sự đến từ cổng thanh toán hay không.
        // Với VNPAY, dữ liệu callback nằm trên query string do trình duyệt gửi lên nên chỉ đáng
        // tin sau khi kiểm chữ ký vnp_SecureHash. Nếu thiếu/sai chữ ký thì đây chỉ là dữ liệu
        // người lạ tự bịa - KHÔNG được phép dùng nó để hủy đơn hàng của khách.
        // Mặc định false để mọi nhánh mới phải chủ động khẳng định tính xác thực.
        public bool IsAuthentic { get; set; } = false;
    }
}
