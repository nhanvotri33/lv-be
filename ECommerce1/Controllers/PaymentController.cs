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
    }
}
