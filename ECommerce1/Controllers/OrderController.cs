using ECommerce.Models;
using ECommerce1.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= XEM DANH SÁCH ĐƠN HÀNG CỦA TÔI =================
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Promotion)
                .Include(o => o.OrderStatus)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    StatusId = o.OrderStatusId,
                    StatusName = o.OrderStatus != null ? o.OrderStatus.Description : "Không xác định",
                    TotalPrice = o.TotalPrice,
                    CreatedAt = o.CreatedAt,
                    UserId = o.UserId,
                    ReceiverName = o.ReceiverName,
                    ReceiverPhone = o.ReceiverPhone,
                    ShippingAddress = $"{o.ShippingAddressLine}, {o.ShippingWard}, {o.ShippingProvince}",
                    PromotionCode = o.Promotion != null ? o.Promotion.Code : null,
                    PointsEarned = o.PointsEarned,
                    PointsRedeemed = o.PointsRedeemed,
                    DiscountFromPoints = o.DiscountFromPoints,
                    Note = o.Note,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        Id = oi.Id,
                        VariantId = oi.VariantId,
                        ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "Sản phẩm không rõ",
                        VariantName = oi.ProductVariant != null ? oi.ProductVariant.Name : "Biến thể không rõ",
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ================= XEM TẤT CẢ ĐƠN HÀNG (ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Promotion)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    StatusId = o.OrderStatusId,
                    StatusName = o.OrderStatus != null ? o.OrderStatus.Description : "Không xác định",
                    TotalPrice = o.TotalPrice,
                    CreatedAt = o.CreatedAt,
                    UserId = o.UserId,
                    ReceiverName = o.ReceiverName,
                    ReceiverPhone = o.ReceiverPhone,
                    ShippingAddress = $"{o.ShippingAddressLine}, {o.ShippingWard}, {o.ShippingProvince}",
                    PromotionCode = o.Promotion != null ? o.Promotion.Code : null,
                    PointsEarned = o.PointsEarned,
                    PointsRedeemed = o.PointsRedeemed,
                    DiscountFromPoints = o.DiscountFromPoints,
                    Note = o.Note,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        Id = oi.Id,
                        VariantId = oi.VariantId,
                        ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "Sản phẩm không rõ",
                        VariantName = oi.ProductVariant != null ? oi.ProductVariant.Name : "Biến thể không rõ",
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ================= ĐẶT HÀNG (CHECKOUT) TỪ GIỎ HÀNG =================
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // 1. Lấy giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return BadRequest("Giỏ hàng của bạn đang trống.");

                // 2. Kiểm tra tồn kho trước khi đặt
                foreach (var item in cart.CartItems)
                {
                    if (item.ProductVariant.AvailableStock < item.Quantity)
                        return BadRequest($"Sản phẩm '{item.ProductVariant.Name}' không đủ tồn kho. Vui lòng giảm số lượng.");
                }

                // 3. Tính tổng tiền
                decimal subTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.ProductVariant.Price);
                decimal discountValue = 0;
                Promotion appliedPromotion = null;

                // 4. Áp dụng mã giảm giá (Nếu có)
                if (!string.IsNullOrEmpty(request.PromotionCode))
                {
                    appliedPromotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.Code == request.PromotionCode && p.IsActive);

                    if (appliedPromotion == null)
                        return BadRequest("Mã giảm giá không tồn tại hoặc đã bị khóa.");

                    if (DateTime.UtcNow < appliedPromotion.StartDate || DateTime.UtcNow > appliedPromotion.EndDate)
                        return BadRequest("Mã giảm giá đã hết hạn hoặc chưa tới thời gian sử dụng.");

                    // Kiểm tra User đã dùng mã này bao giờ chưa 
                    bool hasUsed = await _context.PromotionUsages.AnyAsync(pu => pu.PromotionId == appliedPromotion.Id && pu.UserId == userId);
                    if (hasUsed)
                        return BadRequest("Bạn đã sử dụng mã giảm giá này rồi.");

                    // Kiểm tra giới hạn số lượng mã đã phát hành
                    if (appliedPromotion.UsageLimit > 0 && appliedPromotion.UsedCount >= appliedPromotion.UsageLimit)
                        return BadRequest("Mã giảm giá này đã hết lượt sử dụng.");

                    if (appliedPromotion.DiscountType.ToUpper() == "PERCENTAGE")
                    {
                        discountValue = subTotal * (appliedPromotion.DiscountValue / 100);
                    }
                    else if (appliedPromotion.DiscountType.ToUpper() == "FIXED_AMOUNT")
                    {
                        discountValue = appliedPromotion.DiscountValue;
                    }

                    if (discountValue > subTotal) discountValue = subTotal;
                }

                // 5. Xử lý điểm thành viên và giá thanh toán cuối cùng
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("Không tìm thấy thông tin tài khoản.");

                int pointsRedeemed = 0;
                decimal discountFromPoints = 0;
                decimal priceBeforePoints = subTotal - discountValue;

                if (request.PointsToRedeem > 0)
                {
                    if (user.RewardPoints < request.PointsToRedeem)
                        return BadRequest("Số điểm tích lũy của bạn không đủ.");

                    pointsRedeemed = request.PointsToRedeem;
                    discountFromPoints = pointsRedeemed; // 1 điểm = 1 VNĐ
                    if (discountFromPoints > priceBeforePoints)
                    {
                        discountFromPoints = priceBeforePoints;
                        pointsRedeemed = (int)discountFromPoints;
                    }
                    priceBeforePoints -= discountFromPoints;
                }

                decimal shippingFee = 0;
                string finalWardId = request.WardId;

                if (request.ShippingInfoId.HasValue && request.ShippingInfoId.Value > 0)
                {
                    var shippingInfo = await _context.ShippingInfos.FindAsync(request.ShippingInfoId.Value);
                    if (shippingInfo != null)
                    {
                        finalWardId = shippingInfo.WardId;
                    }
                }

                if (!string.IsNullOrEmpty(finalWardId))
                {
                    var ward = await _context.Wards
                        .Include(w => w.Province)
                        .FirstOrDefaultAsync(w => w.Id == finalWardId);
                    if (ward != null)
                    {
                        decimal baseFee = 35000;
                        string provinceName = ward.Province?.Name ?? "";
                        if (provinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
                            provinceName.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
                            provinceName.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase))
                        {
                            baseFee = 22000;
                        }
                        shippingFee = baseFee;
                    }
                }

                decimal finalPrice = priceBeforePoints + shippingFee;
                if (finalPrice < 0) finalPrice = 0;

                // Tích lũy điểm thưởng: 0.2% trên số tiền thanh toán cuối cùng
                int pointsEarned = (int)(finalPrice * 0.002m);

                if (pointsRedeemed > 0)
                {
                    user.RewardPoints -= pointsRedeemed;
                }

                // 5.2. Xử lý địa chỉ giao hàng (Snapshot)
                string receiverName = "";
                string receiverPhone = "";
                string shippingAddressLine = "";
                string shippingWard = "";
                string shippingProvince = "";

                if (request.ShippingInfoId.HasValue && request.ShippingInfoId.Value > 0)
                {
                    var shippingInfo = await _context.ShippingInfos
                        .Include(s => s.Ward).ThenInclude(w => w.Province)
                        .FirstOrDefaultAsync(s => s.Id == request.ShippingInfoId.Value);

                    if (shippingInfo == null || shippingInfo.UserId != userId)
                        return BadRequest("Địa chỉ không hợp lệ.");

                    receiverName = shippingInfo.RecipientName;
                    receiverPhone = shippingInfo.PhoneNumber;
                    shippingAddressLine = shippingInfo.AddressLine;
                    shippingWard = shippingInfo.Ward != null ? shippingInfo.Ward.Name : "";
                    shippingProvince = shippingInfo.Ward != null && shippingInfo.Ward.Province != null ? shippingInfo.Ward.Province.Name : "";
                }
                else
                {
                    if (string.IsNullOrEmpty(request.RecipientName) || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.AddressLine))
                        return BadRequest("Vui lòng cung cấp đầy đủ thông tin giao hàng.");

                    var ward = await _context.Wards.Include(w => w.Province).FirstOrDefaultAsync(w => w.Id == request.WardId);

                    receiverName = request.RecipientName;
                    receiverPhone = request.PhoneNumber;
                    shippingAddressLine = request.AddressLine;
                    shippingWard = ward != null ? ward.Name : "";
                    shippingProvince = ward != null && ward.Province != null ? ward.Province.Name : "";

                    var newShipping = new ShippingInfo
                    {
                        UserId = userId,
                        RecipientName = request.RecipientName,
                        PhoneNumber = request.PhoneNumber,
                        AddressLine = request.AddressLine,
                        WardId = request.WardId,
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ShippingInfos.Add(newShipping);
                }

                // 6. Tạo đơn hàng (Order)
                var newOrder = new Order
                {
                    UserId = userId,
                    ReceiverName = receiverName,
                    ReceiverPhone = receiverPhone,
                    ShippingAddressLine = shippingAddressLine,
                    ShippingWard = shippingWard,
                    ShippingProvince = shippingProvince,
                    PromotionId = appliedPromotion?.Id,
                    TotalPrice = finalPrice,
                    OrderStatusId = 1, // 1 = Pending (Chờ thanh toán)
                    CreatedAt = DateTime.UtcNow,
                    PointsEarned = pointsEarned,
                    PointsRedeemed = pointsRedeemed,
                    DiscountFromPoints = discountFromPoints,
                    Note = request.Note
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // Lưu để lấy Order.Id

                // 7. Tạo OrderItems và trừ Tồn kho giữ chỗ (ReservedStock)
                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = newOrder.Id,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.ProductVariant.Price
                    };
                    _context.OrderItems.Add(orderItem);

                    // Quan trọng: Tăng ReservedStock lên để giữ hàng cho khách này
                    item.ProductVariant.ReservedStock += item.Quantity;
                }

                // 8. Lưu lịch sử dùng mã giảm giá
                if (appliedPromotion != null)
                {
                    var usage = new PromotionUsage
                    {
                        PromotionId = appliedPromotion.Id,
                        UserId = userId,
                        UsedAt = DateTime.UtcNow
                    };
                    _context.PromotionUsages.Add(usage);
                    
                    // Tăng số lượng đã sử dụng của mã giảm giá
                    appliedPromotion.UsedCount += 1;
                }

                // 9. Xóa giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems);

                // 10. Lưu tất cả thay đổi
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { 
                    Message = "Đặt hàng thành công!", 
                    OrderId = newOrder.Id, 
                    TotalPaid = finalPrice,
                    PointsEarned = pointsEarned,
                    PointsRedeemed = pointsRedeemed,
                    NewPointsBalance = user.RewardPoints
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Có lỗi xảy ra trong quá trình đặt hàng: {ex.Message}");
            }
        }

        // ================= HỦY ĐƠN HÀNG (DÀNH CHO KHÁCH HÀNG) =================
        [HttpPut("{id}/cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelOrder(int id, [FromQuery] string? phoneNumber = null)
        {
            Order order = null;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            }
            else
            {
                if (string.IsNullOrEmpty(phoneNumber))
                    return Unauthorized("Bạn cần đăng nhập hoặc cung cấp số điện thoại nhận hàng để hủy đơn hàng.");

                order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                    .FirstOrDefaultAsync(o => o.Id == id && o.ReceiverPhone == phoneNumber.Trim());
            }

            if (order == null)
                return NotFound("Không tìm thấy đơn hàng của bạn.");

            // Chỉ cho phép hủy nếu đơn hàng đang ở trạng thái Pending (1) (Chờ xác nhận)
            if (order.OrderStatusId != 1)
                return BadRequest("Bạn không thể hủy đơn hàng này vì nó đã được cửa hàng xác nhận và đang đóng gói/giao đi.");

            // Trạng thái 5 là Cancelled (Đã hủy)
            order.OrderStatusId = 5;

            // Xử lý tồn kho: Trả lại ReservedStock cho kho giữ chỗ
            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariant != null)
                {
                    item.ProductVariant.ReservedStock -= item.Quantity;
                    if (item.ProductVariant.ReservedStock < 0) item.ProductVariant.ReservedStock = 0;
                }
            }

            // Hoàn lại điểm đã tiêu dùng cho khách
            var userObj = await _context.Users.FindAsync(order.UserId);
            if (userObj != null && order.PointsRedeemed > 0)
            {
                userObj.RewardPoints += order.PointsRedeemed;
            }

            await _context.SaveChangesAsync();

            return Ok("Hủy đơn hàng thành công.");
        }

        // ================= CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (ADMIN) =================
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] int newStatusId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Không tìm thấy đơn hàng.");

            var statusExists = await _context.OrderStatuses.AnyAsync(s => s.Id == newStatusId);
            if (!statusExists)
                return BadRequest("Trạng thái đơn hàng không hợp lệ.");

            int oldStatusId = order.OrderStatusId;
            if (oldStatusId == newStatusId)
                return Ok("Trạng thái không đổi.");

            // Các trạng thái cuối cùng (Cancelled, Return_failed, Refunded) không cho phép thay đổi nữa
            // Còn Completed chỉ cho phép chuyển sang Refunded
            if (oldStatusId == 5 || oldStatusId == 6 || oldStatusId == 7 || (oldStatusId == 4 && newStatusId != 7))
            {
                return BadRequest("Đơn hàng đã ở trạng thái kết thúc, không thể thay đổi trạng thái này.");
            }

            // Xử lý logic tồn kho (ReservedStock và TotalStock)
            bool oldIsReserving = (oldStatusId == 1 || oldStatusId == 2 || oldStatusId == 3);

            // 1. Chuyển từ Đang giữ hàng -> Completed (Đã giao hàng thành công)
            if (oldIsReserving && newStatusId == 4)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        // Giảm trừ thực tế từ kho tổng và giải phóng kho giữ chỗ
                        item.ProductVariant.TotalStock -= item.Quantity;
                        item.ProductVariant.ReservedStock -= item.Quantity;

                        if (item.ProductVariant.TotalStock < 0) item.ProductVariant.TotalStock = 0;
                        if (item.ProductVariant.ReservedStock < 0) item.ProductVariant.ReservedStock = 0;
                    }
                }

                // Cộng điểm tích lũy vào tài khoản User
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null)
                {
                    user.RewardPoints += order.PointsEarned;
                }
            }
            // 2. Chuyển từ Đang giữ hàng -> Cancelled (Hủy) hoặc Return_failed (Giao hàng thất bại / Hoàn hàng)
            else if (oldIsReserving && (newStatusId == 5 || newStatusId == 6))
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        // Giải phóng kho giữ chỗ, trả lại số lượng AvailableStock cho khách khác mua
                        item.ProductVariant.ReservedStock -= item.Quantity;
                        if (item.ProductVariant.ReservedStock < 0) item.ProductVariant.ReservedStock = 0;
                    }
                }

                // Hoàn lại điểm đã tiêu dùng cho khách
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null && order.PointsRedeemed > 0)
                {
                    user.RewardPoints += order.PointsRedeemed;
                }
            }
            // 3. Chuyển từ Completed (Đã giao) -> Refunded (Đổi trả / Hoàn tiền)
            else if (oldStatusId == 4 && newStatusId == 7)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        // Cộng lại hàng vào kho tổng vì khách đã hoàn trả sản phẩm
                        item.ProductVariant.TotalStock += item.Quantity;
                    }
                }

                // Thu hồi điểm tích lũy và hoàn trả điểm đã tiêu dùng
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null)
                {
                    user.RewardPoints -= order.PointsEarned;
                    if (user.RewardPoints < 0) user.RewardPoints = 0;

                    user.RewardPoints += order.PointsRedeemed;
                }
            }

            order.OrderStatusId = newStatusId;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật trạng thái đơn hàng và xử lý tồn kho thành công.");
        }

        // ================= TRA CỨU ĐƠN HÀNG (DÀNH CHO KHÁCH VÃNG LAI) =================
        [HttpGet("track")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackOrder([FromQuery] string orderId, [FromQuery] string phoneNumber)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(phoneNumber))
            {
                return BadRequest("Vui lòng cung cấp mã đơn hàng và số điện thoại.");
            }

            if (!int.TryParse(orderId, out int id))
            {
                return BadRequest("Mã đơn hàng không hợp lệ.");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Promotion)
                .Include(o => o.OrderStatus)
                .FirstOrDefaultAsync(o => o.Id == id && o.ReceiverPhone == phoneNumber.Trim());

            if (order == null)
            {
                return NotFound("Không tìm thấy thông tin đơn hàng hoặc số điện thoại không khớp.");
            }

            var response = new OrderResponse
            {
                Id = order.Id,
                StatusId = order.OrderStatusId,
                StatusName = order.OrderStatus != null ? order.OrderStatus.Description : "Không xác định",
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt,
                UserId = order.UserId,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                ShippingAddress = $"{order.ShippingAddressLine}, {order.ShippingWard}, {order.ShippingProvince}",
                PaymentMethod = order.PaymentMethod,
                PromotionCode = order.Promotion != null ? order.Promotion.Code : null,
                PointsEarned = order.PointsEarned,
                PointsRedeemed = order.PointsRedeemed,
                DiscountFromPoints = order.DiscountFromPoints,
                Items = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "Sản phẩm không rõ",
                    VariantName = oi.ProductVariant != null ? oi.ProductVariant.Name : "Biến thể không rõ",
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase
                }).ToList()
            };

            return Ok(response);
        }
    }
}
