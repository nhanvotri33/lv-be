// ==========================================================================
// MODULE: PaymentController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module PaymentController
// ==========================================================================
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ECommerce1.Services;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
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
        private readonly IConfiguration _configuration;

        public PaymentController(
            ApplicationDbContext context, 
            IEnumerable<ECommerce1.Services.Payment.IPaymentProvider> paymentProviders,
            IConfiguration configuration)
        {
            _context = context;
            _paymentProviders = paymentProviders;
            _configuration = configuration;
        }

        // [API Endpoint POST [Route: `create-checkout-session/{orderId}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPost("create-checkout-session/{orderId}")]
        // [Hàm thực thi nghiệp vụ]: `CreateCheckoutSession` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CreateCheckoutSession(int orderId, [FromQuery] string provider = "stripe")
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Promotion)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
            if (order == null) return NotFound("Không tìm thấy đơn hàng");
            // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
            if (order.OrderStatusId != 1) return BadRequest("Đơn hàng này không ở trạng thái chờ thanh toán.");

            // Đồng bộ TotalPrice trước khi gửi cổng thanh toán:
            // Recompute canonical total từ OrderItems (đã bao gồm giảm combo qua PriceAtPurchase) + WarrantyPrice
            // - promotion discount + shipping fee - discount điểm thưởng.
            // Chỉ thực hiện khi đơn có OrderItems và giá trị recompute là số hợp lệ (>= 0)
            // để không chạm vào đơn ở trạng thái bất thường.
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                var itemsSubTotal = order.OrderItems.Sum(oi => (oi.PriceAtPurchase + oi.WarrantyPrice) * oi.Quantity);
                decimal promoDiscount = 0;
                if (order.Promotion != null && !string.IsNullOrEmpty(order.Promotion.DiscountType))
                {
                    var promo = order.Promotion;
                    var type = promo.DiscountType.ToUpper();
                    if (type == "PERCENTAGE")
                    {
                        promoDiscount = itemsSubTotal * (promo.DiscountValue / 100m);
                        if (promo.MaxDiscountAmount.HasValue && promoDiscount > promo.MaxDiscountAmount.Value)
                            promoDiscount = promo.MaxDiscountAmount.Value;
                    }
                    else if (type == "FIXED_AMOUNT")
                    {
                        promoDiscount = promo.DiscountValue;
                    }
                    // Type lạ -> không suy đoán, giữ promoDiscount = 0

                    if (promoDiscount < 0) promoDiscount = 0;
                    if (promoDiscount > itemsSubTotal) promoDiscount = itemsSubTotal;
                }
                var pointsDiscount = order.DiscountFromPoints < 0 ? 0 : order.DiscountFromPoints;
                var shippingFee = order.ActualShippingFee.HasValue && order.ActualShippingFee.Value > 0
                    ? order.ActualShippingFee.Value
                    : 0m;
                var expectedTotal = Math.Max(0, itemsSubTotal - promoDiscount - pointsDiscount + shippingFee);
                if (order.TotalPrice != expectedTotal)
                {
                    order.TotalPrice = expectedTotal;
                    await _context.SaveChangesAsync();
                }
            }

            // Hỗ trợ lưu log giao dịch COD trong Nhật ký giao dịch thanh toá 
            //if (provider.Equals("cod", StringComparison.OrdinalIgnoreCase))
            //{
            //    try
            //    {
            //        var payment = new Payment
            //        {
            //            OrderId = order.Id,
            //            UserId = userId,
            //            Provider = "cod",
            //            ProviderSessionId = $"COD-{Guid.NewGuid()}",
            //            ProviderTransactionId = $"COD-{DateTime.UtcNow.Ticks}",
            //            Amount = order.TotalPrice,
            //            Currency = "vnd",
            //            Status = "pending",
            //            CreatedAt = DateTime.UtcNow,
            //            UpdatedAt = DateTime.UtcNow
            //        };

            //        _context.Payments.Add(payment);
            //        await _context.SaveChangesAsync();

            //        return Ok(new { url = "" });
            //    }
            //    catch (Exception ex)
            //    {
            //        return BadRequest(new { message = ex.Message });
            //    }
            //}

            var paymentProvider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));
            // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
            if (paymentProvider == null) return BadRequest($"Phương thức thanh toán '{provider}' không được hỗ trợ.");

            var domain = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var successUrl = provider.Equals("vnpay", StringComparison.OrdinalIgnoreCase)
                ? domain + "/payment-callback?provider=vnpay"
                : domain + $"/payment-callback?session_id={{CHECKOUT_SESSION_ID}}&provider={provider}";
            var cancelUrl = domain + $"/payment-callback?cancel=true&provider={provider}";

            try
            {
                // Gọi tới Provider để tạo phiên thanh toán (trả về URL hoặc mã giao dịch)
                // Lưu ý: Stripe yêu cầu {CHECKOUT_SESSION_ID} trong successUrl để thay thế tự động
                var checkoutSession = await paymentProvider.CreateCheckoutSessionAsync(order, successUrl, cancelUrl);
                var sessionId = provider.Equals("vnpay", StringComparison.OrdinalIgnoreCase)
                    ? order.Id.ToString()
                    : checkoutSession;

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

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Payments.Add(payment);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // Trả về URL thanh toán tương ứng cho từng cổng thanh toán
                if (provider.Equals("stripe", StringComparison.OrdinalIgnoreCase))
                {
                    if (sessionId.StartsWith("mock_stripe_session_"))
                    {
                        var mockRedirectUrl = successUrl.Replace("{CHECKOUT_SESSION_ID}", sessionId);
                        // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                        return Ok(new { url = mockRedirectUrl });
                    }
                    var service = new Stripe.Checkout.SessionService();
                    var sessionInfo = await service.GetAsync(sessionId);
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { url = sessionInfo.Url });
                }
                else if (provider.Equals("vnpay", StringComparison.OrdinalIgnoreCase))
                {
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { url = checkoutSession });
                }

                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Phương thức thanh toán không hợp lệ.");
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = ex.Message });
            }
        }

        // [API Endpoint GET [Route: `verify-session`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("verify-session")]
        [AllowAnonymous]
        // [Hàm thực thi nghiệp vụ]: `VerifySession` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> VerifySession([FromQuery] string? session_id, [FromQuery] string provider = "stripe")
        {
            try
            {
                if (provider.Equals("vnpay", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(session_id))
                    session_id = Request.Query["vnp_TxnRef"].ToString();

                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                if (string.IsNullOrWhiteSpace(session_id)) return BadRequest("Không tìm thấy mã phiên giao dịch.");

                var paymentProvider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                if (paymentProvider == null) return BadRequest($"Phương thức thanh toán '{provider}' không được hỗ trợ.");

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ProviderSessionId == session_id && p.Provider == provider);
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                if (payment == null) return NotFound("Không tìm thấy giao dịch.");

                var result = await paymentProvider.VerifySessionAsync(session_id);

                if (result.IsSuccess)
                {
                    payment.Status = "succeeded";
                    payment.ProviderTransactionId = result.TransactionId ?? "";
                    payment.UpdatedAt = DateTime.UtcNow;

                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    var order = await _context.Orders.FindAsync(payment.OrderId);
                    if (order != null && order.OrderStatusId == 1)
                    {
                        order.OrderStatusId = 2; // Processing
                    }

                    // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                    await _context.SaveChangesAsync();
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { message = result.Message, orderId = order?.Id });
                }

                payment.Status = "failed";
                payment.UpdatedAt = DateTime.UtcNow;
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = ex.Message });
            }
        }

        // [API Endpoint POST [Route: `cancel-session`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPost("cancel-session")]
        // [Hàm thực thi nghiệp vụ]: `CancelSession` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CancelSession([FromQuery] string session_id, [FromQuery] string provider = "stripe")
        {
            try
            {
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ProviderSessionId == session_id && p.Provider == provider);
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                if (payment == null) return NotFound("Không tìm thấy giao dịch.");

                if (payment.Status == "pending")
                {
                    payment.Status = "failed";
                    payment.UpdatedAt = DateTime.UtcNow;
                    // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                    await _context.SaveChangesAsync();
                }

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { message = "Giao dịch đã được hủy." });
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = ex.Message });
            }
        }

        // [API Endpoint GET [Route: `admin/all-payments`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("admin/all-payments")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetAllPayments` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Order)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new {
                        p.Id,
                        p.OrderId,
                        CustomerName = p.User != null ? p.User.Username : "N/A",
                        CustomerEmail = p.User != null ? p.User.Email : "N/A",
                        p.Provider,
                        p.ProviderSessionId,
                        p.ProviderTransactionId,
                        p.Amount,
                        p.Currency,
                        p.Status,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(payments);
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = ex.Message });
            }
        }

        // [API Endpoint GET [Route: `vnpay-ipn`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("vnpay-ipn")]
        [AllowAnonymous]
        // [Hàm thực thi nghiệp vụ]: `VnPayIpn` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> VnPayIpn()
        {
            try
            {
                var query = Request.Query;
                var txnRef = query["vnp_TxnRef"].ToString();
                var vnpayProvider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals("vnpay", StringComparison.OrdinalIgnoreCase));

                if (vnpayProvider == null || string.IsNullOrWhiteSpace(txnRef))
                {
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ProviderSessionId == txnRef && p.Provider == "vnpay");
                if (payment == null)
                {
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                var order = await _context.Orders.FindAsync(payment.OrderId);
                if (order == null)
                {
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                if (order.OrderStatusId != 1)
                {
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { RspCode = "02", Message = "Order already confirmed" });
                }

                if (query.ContainsKey("vnp_Amount") && long.TryParse(query["vnp_Amount"], out long vnpAmount))
                {
                    long expectedAmount = (long)Math.Round(order.TotalPrice * 100, 0);
                    if (vnpAmount != expectedAmount)
                    {
                        // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                        return Ok(new { RspCode = "04", Message = "Invalid amount" });
                    }
                }

                var verificationResult = await vnpayProvider.VerifySessionAsync(txnRef);
                if (!verificationResult.IsSuccess && verificationResult.Message == "Chữ ký VNPAY không hợp lệ.")
                {
                    // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                    return Ok(new { RspCode = "97", Message = "Invalid Checksum" });
                }

                if (verificationResult.IsSuccess)
                {
                    payment.Status = "succeeded";
                    payment.ProviderTransactionId = verificationResult.TransactionId ?? "";
                    payment.UpdatedAt = DateTime.UtcNow;
                    order.OrderStatusId = 2; // Processing (Đã thanh toán)
                    // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                    await _context.SaveChangesAsync();
                }
                else
                {
                    payment.Status = "failed";
                    payment.UpdatedAt = DateTime.UtcNow;
                    // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                    await _context.SaveChangesAsync();
                }

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch
            {
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }
    }
}
