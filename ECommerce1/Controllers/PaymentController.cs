using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<ECommerce1.Services.Payment.IPaymentProvider> _paymentProviders;

        public PaymentController(ApplicationDbContext context, IEnumerable<ECommerce1.Services.Payment.IPaymentProvider> paymentProviders)
        {
            _context = context;
            _paymentProviders = paymentProviders;
        }

        [HttpPost("create-checkout-session/{orderId}")]
        public async Task<IActionResult> CreateCheckoutSession(int orderId, [FromQuery] string provider = "stripe")
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return NotFound("Không tìm thấy đơn hàng");
            if (order.OrderStatusId != 1) return BadRequest("Đơn hàng này không ở trạng thái chờ thanh toán.");

            var paymentProvider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));
            if (paymentProvider == null) return BadRequest($"Phương thức thanh toán '{provider}' không được hỗ trợ.");

            var domain = "http://localhost:5173"; 
            var successUrl = domain + $"/payment-callback?session_id={{CHECKOUT_SESSION_ID}}&provider={provider}";
            var cancelUrl = domain + $"/payment-callback?cancel=true&provider={provider}";

            try
            {
                // Gọi tới Provider để tạo phiên thanh toán (trả về URL hoặc mã giao dịch)
                // Lưu ý: Stripe yêu cầu {CHECKOUT_SESSION_ID} trong successUrl để thay thế tự động
                var sessionId = await paymentProvider.CreateCheckoutSessionAsync(order, successUrl, cancelUrl);

                // Stripe trả về sessionId thay vì URL, vì ta dùng CreateAsync ở Provider
                // Nhưng Frontend đang mong đợi 1 đối tượng có { url }
                // Để đơn giản, đối với Stripe ta có thể trả về Session URL từ Provider
                // Sửa Provider sau, tạm thời tạo Payment trong DB
                
                var payment = new Payment
                {
                    OrderId = order.Id,
                    UserId = userId,
                    Provider = provider,
                    ProviderSessionId = sessionId,
                    ProviderTransactionId = "",
                    Amount = order.TotalPrice,
                    Currency = "vnd",
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Ở bước này, với Stripe, sessionId có thể được frontend dùng với StripeJS,
                // Nhưng Frontend của ta đang redirect tới `url`.
                // Vì vậy, ta cần trả về URL tương ứng. Với Stripe service, Session url có dạng https://checkout.stripe.com/c/pay/cs_test_...
                // Thực tế Stripe Session service get trả về Url.
                var service = new Stripe.Checkout.SessionService();
                var sessionInfo = await service.GetAsync(sessionId);

                return Ok(new { url = sessionInfo.Url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verify-session")]
        public async Task<IActionResult> VerifySession([FromQuery] string session_id, [FromQuery] string provider = "stripe")
        {
            try
            {
                var paymentProvider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));
                if (paymentProvider == null) return BadRequest($"Phương thức thanh toán '{provider}' không được hỗ trợ.");

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ProviderSessionId == session_id && p.Provider == provider);
                if (payment == null) return NotFound("Không tìm thấy giao dịch.");

                var result = await paymentProvider.VerifySessionAsync(session_id);

                if (result.IsSuccess)
                {
                    payment.Status = "succeeded";
                    payment.ProviderTransactionId = result.TransactionId ?? "";
                    payment.UpdatedAt = DateTime.UtcNow;

                    var order = await _context.Orders.FindAsync(payment.OrderId);
                    if (order != null && order.OrderStatusId == 1)
                    {
                        order.OrderStatusId = 2; // Processing
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new { message = result.Message, orderId = order?.Id });
                }

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================= MOMO WEBHOOK IPN ENDPOINT =================
        [HttpPost("momo-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> MomoWebhook([FromBody] MomoWebhookRequest request)
        {
            try
            {
                // Giả lập khóa bảo mật của MoMo (Secret Key)
                var secretKey = "your_momo_secret_key";
                
                // Tạo chuỗi raw data để đối chiếu chữ ký (theo quy chuẩn tài liệu tích hợp MoMo)
                var rawData = $"accessKey=mock_access_key&amount={request.Amount}&extraData={request.ExtraData}&message={request.Message}&orderId={request.OrderId}&orderInfo={request.OrderInfo}&orderType={request.OrderType}&partnerCode={request.PartnerCode}&requestId={request.RequestId}&responseTime={request.ResponseTime}&resultCode={request.ResultCode}&transId={request.TransId}";
                
                // Tính toán HMAC SHA256 để đối chiếu bảo mật
                var computedSignature = ComputeHmacSha256(rawData, secretKey);
                
                // Cập nhật trạng thái đơn hàng nếu giao dịch thành công (ResultCode = 0)
                if (request.ResultCode == 0)
                {
                    // Chuyển đổi mã đơn hàng (ví dụ: ORDER_123 hoặc 123)
                    var cleanOrderId = request.OrderId.Replace("ORDER_", "");
                    if (int.TryParse(cleanOrderId, out int systemOrderId))
                    {
                        var order = await _context.Orders.FindAsync(systemOrderId);
                        if (order != null && order.OrderStatusId == 1) // Chờ thanh toán
                        {
                            order.OrderStatusId = 2; // Đang xử lý
                            
                            var payment = new Payment
                            {
                                OrderId = order.Id,
                                UserId = order.UserId,
                                Provider = "momo",
                                ProviderSessionId = request.RequestId,
                                ProviderTransactionId = request.TransId,
                                Amount = request.Amount,
                                Currency = "vnd",
                                Status = "succeeded",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            
                            _context.Payments.Add(payment);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Trả về NoContent báo cho MoMo là đã xử lý Webhook thành công
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string ComputeHmacSha256(string message, string key)
        {
            var keyByte = System.Text.Encoding.UTF8.GetBytes(key);
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte))
            {
                var hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }

    public class MomoWebhookRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string TransId { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PayType { get; set; } = string.Empty;
        public string ResponseTime { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }
}
