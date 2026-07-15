using ECommerce.Models;
using ECommerce1.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<ECommerce1.Services.Payment.IPaymentProvider> _paymentProviders;
        private readonly IAhamoveService _ahamoveService;

        public OrderService(ApplicationDbContext context, 
            IEnumerable<ECommerce1.Services.Payment.IPaymentProvider> paymentProviders,
            IAhamoveService ahamoveService)
        {
            _context = context;
            _paymentProviders = paymentProviders;
            _ahamoveService = ahamoveService;
        }

        public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid userId)
        {
            return await _context.Orders
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
                    PaymentMethod = o.PaymentMethod,
                    PromotionCode = o.Promotion != null ? o.Promotion.Code : null,
                    PointsEarned = o.PointsEarned,
                    PointsRedeemed = o.PointsRedeemed,
                    DiscountFromPoints = o.DiscountFromPoints,
                    Note = o.Note,
                    DeliveryLatitude = o.DeliveryLatitude,
                    DeliveryLongitude = o.DeliveryLongitude,
                    AhamoveOrderId = o.AhamoveOrderId,
                    AhamoveStatus = o.AhamoveStatus,
                    AhamoveSharedLink = o.AhamoveSharedLink,
                    ActualShippingFee = o.ActualShippingFee,
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
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
        {
            return await _context.Orders
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
                    PaymentMethod = o.PaymentMethod,
                    PromotionCode = o.Promotion != null ? o.Promotion.Code : null,
                    PointsEarned = o.PointsEarned,
                    PointsRedeemed = o.PointsRedeemed,
                    DiscountFromPoints = o.DiscountFromPoints,
                    Note = o.Note,
                    DeliveryLatitude = o.DeliveryLatitude,
                    DeliveryLongitude = o.DeliveryLongitude,
                    AhamoveOrderId = o.AhamoveOrderId,
                    AhamoveStatus = o.AhamoveStatus,
                    AhamoveSharedLink = o.AhamoveSharedLink,
                    ActualShippingFee = o.ActualShippingFee,
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
        }

        public async Task<object> CheckoutAsync(Guid userId, CheckoutRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // 1. Lấy giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    throw new ArgumentException("Giỏ hàng của bạn đang trống.");

                // 2. Kiểm tra tồn kho trước khi đặt
                foreach (var item in cart.CartItems)
                {
                    if (item.ProductVariant.AvailableStock < item.Quantity)
                        throw new ArgumentException($"Sản phẩm '{item.ProductVariant.Name}' không đủ tồn kho. Vui lòng giảm số lượng.");
                }

                // 3. Tính tổng tiền & Xử lý giá Combo
                decimal subTotal = 0;
                var calculatedPrices = new System.Collections.Generic.Dictionary<int, decimal>();

                var cartItemsList = cart.CartItems.ToList();
                foreach (var item in cartItemsList)
                {
                    decimal price = item.ProductVariant.Price;

                    if (item.AppliedComboId.HasValue)
                    {
                        int comboId = item.AppliedComboId.Value;
                        var comboConfig = await _context.ProductComboItems.Where(c => c.ProductComboId == comboId).ToListAsync();
                        var mainProductIds = comboConfig.Where(c => c.IsMain).Select(c => c.ProductId).ToList();
                        bool hasMain = cartItemsList.Any(ci => ci.AppliedComboId == comboId && mainProductIds.Contains(ci.ProductVariant.ProductId));

                        if (hasMain)
                        {
                            var config = comboConfig.FirstOrDefault(c => c.ProductId == item.ProductVariant.ProductId);
                            if (config != null && !config.IsMain)
                            {
                                if (config.DiscountType == "Percentage")
                                    price = price * (1 - config.DiscountValue / 100);
                                else if (config.DiscountType == "FixedAmount")
                                    price = Math.Max(0, price - config.DiscountValue);
                            }
                        }
                        else
                        {
                            item.AppliedComboId = null;
                        }
                    }

                    calculatedPrices[item.Id] = price;
                    subTotal += price * item.Quantity;
                }

                decimal discountValue = 0;
                Promotion appliedPromotion = null;

                // 4. Áp dụng mã giảm giá (Nếu có)
                if (!string.IsNullOrEmpty(request.PromotionCode))
                {
                    appliedPromotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.Code == request.PromotionCode && p.IsActive);

                    if (appliedPromotion == null)
                        throw new ArgumentException("Mã giảm giá không tồn tại hoặc đã bị khóa.");

                    if (DateTime.UtcNow < appliedPromotion.StartDate || DateTime.UtcNow > appliedPromotion.EndDate)
                        throw new ArgumentException("Mã giảm giá đã hết hạn hoặc chưa tới thời gian sử dụng.");

                    // Kiểm tra User đã dùng mã này bao giờ chưa 
                    bool hasUsed = await _context.PromotionUsages.AnyAsync(pu => pu.PromotionId == appliedPromotion.Id && pu.UserId == userId);
                    if (hasUsed)
                        throw new ArgumentException("Bạn đã sử dụng mã giảm giá này rồi.");

                    // Kiểm tra giới hạn số lượng mã đã phát hành
                    if (appliedPromotion.UsageLimit > 0 && appliedPromotion.UsedCount >= appliedPromotion.UsageLimit)
                        throw new ArgumentException("Mã giảm giá này đã hết lượt sử dụng.");

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
                if (user == null) 
                    throw new KeyNotFoundException("Không tìm thấy thông tin tài khoản.");

                int pointsRedeemed = 0;
                decimal discountFromPoints = 0;
                decimal priceBeforePoints = subTotal - discountValue;

                if (request.PointsToRedeem > 0)
                {
                    if (user.RewardPoints < request.PointsToRedeem)
                        throw new ArgumentException("Số điểm tích lũy của bạn không đủ.");

                    pointsRedeemed = request.PointsToRedeem;
                    discountFromPoints = pointsRedeemed; // 1 điểm = 1 VNĐ
                    if (discountFromPoints > priceBeforePoints)
                    {
                        discountFromPoints = priceBeforePoints;
                        pointsRedeemed = (int)discountFromPoints;
                    }
                    priceBeforePoints -= discountFromPoints;
                }

                // 5.2. Xử lý địa chỉ giao hàng (Snapshot) & Tọa độ giao hàng
                string receiverName = "";
                string receiverPhone = "";
                string shippingAddressLine = "";
                string shippingWard = "";
                string shippingProvince = "";
                double? deliveryLat = null;
                double? deliveryLng = null;

                if (request.ShippingInfoId.HasValue && request.ShippingInfoId.Value > 0)
                {
                    var shippingInfo = await _context.ShippingInfos
                        .Include(s => s.Ward).ThenInclude(w => w.Province)
                        .FirstOrDefaultAsync(s => s.Id == request.ShippingInfoId.Value);

                    if (shippingInfo == null || shippingInfo.UserId != userId)
                        throw new ArgumentException("Địa chỉ không hợp lệ.");

                    receiverName = shippingInfo.RecipientName;
                    receiverPhone = shippingInfo.PhoneNumber;
                    shippingAddressLine = shippingInfo.AddressLine;
                    shippingWard = shippingInfo.Ward != null ? shippingInfo.Ward.Name : "";
                    shippingProvince = shippingInfo.Ward != null && shippingInfo.Ward.Province != null ? shippingInfo.Ward.Province.Name : "";
                    deliveryLat = shippingInfo.Latitude;
                    deliveryLng = shippingInfo.Longitude;
                }
                else
                {
                    if (string.IsNullOrEmpty(request.RecipientName) || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.AddressLine) || string.IsNullOrEmpty(request.WardId))
                        throw new ArgumentException("Vui lòng cung cấp đầy đủ thông tin giao hàng.");

                    await ECommerce1.Services.VietnamLocationService.EnsureLocationExistsAsync(_context, request.WardId);

                    var ward = await _context.Wards.Include(w => w.Province).FirstOrDefaultAsync(w => w.Id == request.WardId);

                    receiverName = request.RecipientName;
                    receiverPhone = request.PhoneNumber;
                    shippingAddressLine = request.AddressLine;
                    shippingWard = ward != null ? ward.Name : "";
                    shippingProvince = ward != null && ward.Province != null ? ward.Province.Name : "";
                    deliveryLat = request.DeliveryLatitude;
                    deliveryLng = request.DeliveryLongitude;
                }

                // 5.3. Tính phí giao hàng (Estimate Fee) qua Ahamove
                decimal shippingFee = 0;
                bool isAhamoveCalculated = false;

                if (deliveryLat.HasValue && deliveryLng.HasValue)
                {
                    try
                    {
                        var destAddress = $"{shippingAddressLine}, {shippingWard}, {shippingProvince}";
                        shippingFee = await _ahamoveService.EstimateFeeAsync(deliveryLat.Value, deliveryLng.Value, destAddress);
                        isAhamoveCalculated = true;
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi nếu cần thiết, ở đây chúng ta sẽ fallback về tính phí mặc định để không block khách thanh toán
                        Console.WriteLine($"Lỗi gọi API Ahamove: {ex.Message}. Sử dụng cách tính phí mặc định làm phương án dự phòng.");
                    }
                }

                // Fallback nếu không có tọa độ hoặc API Ahamove gặp sự cố
                if (!isAhamoveCalculated)
                {
                    decimal baseFee = 35000;
                    if (shippingProvince.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
                        shippingProvince.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
                        shippingProvince.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase))
                    {
                        baseFee = 22000;
                    }
                    shippingFee = baseFee;
                }

                decimal finalPrice = priceBeforePoints + shippingFee;
                if (finalPrice < 0) finalPrice = 0;

                // Tích lũy điểm thưởng: 0.2% trên số tiền thanh toán cuối cùng
                int pointsEarned = (int)(finalPrice * 0.002m);

                if (pointsRedeemed > 0)
                {
                    user.RewardPoints -= pointsRedeemed;
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
                    DeliveryLatitude = deliveryLat,
                    DeliveryLongitude = deliveryLng,
                    PromotionId = appliedPromotion?.Id,
                    TotalPrice = finalPrice,
                    OrderStatusId = 1, // 1 = Pending (Chờ thanh toán)
                    CreatedAt = DateTime.UtcNow,
                    PointsEarned = pointsEarned,
                    PointsRedeemed = pointsRedeemed,
                    DiscountFromPoints = discountFromPoints,
                    Note = request.Note,
                    PaymentMethod = request.PaymentMethod ?? "COD"
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // Lưu để lấy Order.Id

                // 7. Tạo OrderItems và trừ Tồn kho giữ chỗ (ReservedStock)
                foreach (var item in cart.CartItems)
                {
                    decimal finalItemPrice = calculatedPrices.ContainsKey(item.Id) ? calculatedPrices[item.Id] : item.ProductVariant.Price;
                    decimal comboDiscountAmt = item.ProductVariant.Price - finalItemPrice;

                    var orderItem = new OrderItem
                    {
                        OrderId = newOrder.Id,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = finalItemPrice,
                        AppliedComboId = item.AppliedComboId,
                        ComboDiscountAmount = comboDiscountAmt
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

                return new { 
                    Message = "Đặt hàng thành công!", 
                    OrderId = newOrder.Id, 
                    TotalPaid = finalPrice,
                    PointsEarned = pointsEarned,
                    PointsRedeemed = pointsRedeemed,
                    NewPointsBalance = user.RewardPoints,
                    NewAccumulatedPoints = user.AccumulatedPoints
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelOrderAsync(int id, Guid? userId, string? phoneNumber)
        {
            Order order = null;

            if (userId.HasValue)
            {
                order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);
            }
            else
            {
                if (string.IsNullOrEmpty(phoneNumber))
                    throw new UnauthorizedAccessException("Bạn cần đăng nhập hoặc cung cấp số điện thoại nhận hàng để hủy đơn hàng.");

                order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                    .FirstOrDefaultAsync(o => o.Id == id && o.ReceiverPhone == phoneNumber.Trim());
            }

            if (order == null)
                throw new KeyNotFoundException("Không tìm thấy đơn hàng của bạn.");

            // Chỉ cho phép hủy nếu đơn hàng đang ở trạng thái Pending (1) (Chờ xác nhận)
            if (order.OrderStatusId != 1)
                throw new ArgumentException("Bạn không thể hủy đơn hàng này vì nó đã được cửa hàng xác nhận và đang đóng gói/giao đi.");

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
        }

        public async Task UpdateOrderStatusAsync(int id, int newStatusId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

            var statusExists = await _context.OrderStatuses.AnyAsync(s => s.Id == newStatusId);
            if (!statusExists)
                throw new ArgumentException("Trạng thái đơn hàng không hợp lệ.");

            int oldStatusId = order.OrderStatusId;
            if (oldStatusId == newStatusId)
                return; // Trạng thái không đổi

            // Các trạng thái cuối cùng (Cancelled, Return_failed, Refunded) không cho phép thay đổi nữa
            // Còn Completed chỉ cho phép chuyển sang Refunded
            if (oldStatusId == 5 || oldStatusId == 6 || oldStatusId == 7 || (oldStatusId == 4 && newStatusId != 7))
            {
                throw new ArgumentException("Đơn hàng đã ở trạng thái kết thúc, không thể thay đổi trạng thái này.");
            }

            // Xử lý logic tồn kho (ReservedStock và TotalStock)
            // 1. Chuyển từ Chờ duyệt (1) sang Đã duyệt/Đang giao/Hoàn thành (2, 3, 4) -> Trừ kho luôn
            if (oldStatusId == 1 && (newStatusId == 2 || newStatusId == 3 || newStatusId == 4))
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.TotalStock -= item.Quantity;
                        item.ProductVariant.ReservedStock -= item.Quantity;

                        if (item.ProductVariant.TotalStock < 0) item.ProductVariant.TotalStock = 0;
                        if (item.ProductVariant.ReservedStock < 0) item.ProductVariant.ReservedStock = 0;
                    }
                }
            }
            // 2. Chuyển từ các trạng thái đã xác nhận/đang giao (2, 3) sang Hủy (5), Thất bại (6) hoặc Hoàn tiền (7) -> Hoàn trả kho tổng (vì đã trừ ở bước 1)
            else if ((oldStatusId == 2 || oldStatusId == 3) && (newStatusId == 5 || newStatusId == 6 || newStatusId == 7))
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.TotalStock += item.Quantity;
                    }
                }
            }
            // 3. Chuyển từ Chờ duyệt (1) sang Hủy (5), Thất bại (6) hoặc Hoàn tiền (7) -> Chỉ giải phóng kho giữ chỗ (vì chưa trừ kho tổng)
            else if (oldStatusId == 1 && (newStatusId == 5 || newStatusId == 6 || newStatusId == 7))
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.ReservedStock -= item.Quantity;
                        if (item.ProductVariant.ReservedStock < 0) item.ProductVariant.ReservedStock = 0;
                    }
                }
            }

            // Đồng bộ Product.TotalStock và Product.ReservedStock từ tổng các Variant
            var affectedProductIds = order.OrderItems
                .Where(i => i.ProductVariant != null)
                .Select(i => i.ProductVariant!.ProductId)
                .Distinct()
                .ToList();

            foreach (var productId in affectedProductIds)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    var variants = await _context.ProductVariants
                        .Where(pv => pv.ProductId == productId)
                        .ToListAsync();

                    product.TotalStock = variants.Sum(pv => pv.TotalStock);
                    product.ReservedStock = variants.Sum(pv => pv.ReservedStock);
                }
            }

            // Xử lý cộng điểm tích lũy khi hoàn thành đơn
            if (newStatusId == 4 && oldStatusId != 4)
            {
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null)
                {
                    user.RewardPoints += order.PointsEarned;
                    user.AccumulatedPoints += order.PointsEarned;
                }
            }

            // Xử lý hoàn điểm khi hủy đơn hoặc hoàn tiền
            if ((newStatusId == 5 || newStatusId == 6 || newStatusId == 7) && (oldStatusId == 1 || oldStatusId == 2 || oldStatusId == 3))
            {
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null && order.PointsRedeemed > 0)
                {
                    user.RewardPoints += order.PointsRedeemed;
                }
            }
            // 4. Chuyển từ Completed (Đã giao) thành Refunded (Đổi trả và Hoàn tiền)
            else if (oldStatusId == 4 && newStatusId == 7)
            {
                // Thu hồi điểm tích lũy và hoàn trả điểm đã tiêu dùng (Không hoàn lại kho tồn máy mới)
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null)
                {
                    user.RewardPoints -= order.PointsEarned;
                    if (user.RewardPoints < 0) user.RewardPoints = 0;

                    user.AccumulatedPoints -= order.PointsEarned;
                    if (user.AccumulatedPoints < 0) user.AccumulatedPoints = 0;

                    user.RewardPoints += order.PointsRedeemed;
                }
            }

            // Hoàn tiền tự động qua cổng thanh toán Stripe nếu có
            if (newStatusId == 7)
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == order.Id && p.Status == "succeeded" && p.Provider == "stripe");

                if (payment != null && !string.IsNullOrEmpty(payment.ProviderTransactionId))
                {
                    var stripeProvider = _paymentProviders.FirstOrDefault(p => p.ProviderName.Equals("stripe", StringComparison.OrdinalIgnoreCase));
                    if (stripeProvider != null)
                    {
                        try
                        {
                            bool refundSuccess = await stripeProvider.RefundAsync(payment.ProviderTransactionId, payment.Amount);
                            if (refundSuccess)
                            {
                                payment.Status = "refunded";
                                payment.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Lỗi tự động hoàn tiền qua Stripe: {ex.Message}. Vui lòng kiểm tra lại cấu hình hoặc hoàn tiền thủ công.", ex);
                        }
                    }
                }
            }

            order.OrderStatusId = newStatusId;
            await _context.SaveChangesAsync();
        }

        public async Task<OrderResponse> TrackOrderAsync(int id, string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                throw new ArgumentException("Vui lòng cung cấp số điện thoại.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Promotion)
                .Include(o => o.OrderStatus)
                .FirstOrDefaultAsync(o => o.Id == id && o.ReceiverPhone == phoneNumber.Trim());

            if (order == null)
                throw new KeyNotFoundException("Không tìm thấy thông tin đơn hàng hoặc số điện thoại không khớp.");

            return new OrderResponse
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
                Note = order.Note,
                DeliveryLatitude = order.DeliveryLatitude,
                DeliveryLongitude = order.DeliveryLongitude,
                AhamoveOrderId = order.AhamoveOrderId,
                AhamoveStatus = order.AhamoveStatus,
                AhamoveSharedLink = order.AhamoveSharedLink,
                ActualShippingFee = order.ActualShippingFee,
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
        }

        public async Task<OrderResponse> ShipWithAhamoveAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.OrderStatus)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

            if (!string.IsNullOrEmpty(order.AhamoveOrderId))
                throw new InvalidOperationException("Đơn hàng này đã được gửi sang Ahamove trước đó.");

            if (!order.DeliveryLatitude.HasValue || !order.DeliveryLongitude.HasValue)
                throw new InvalidOperationException("Đơn hàng chưa có tọa độ kinh độ/vĩ độ (Lat/Lng) để giao hàng. Vui lòng cập nhật tọa độ.");

            // Gọi dịch vụ Ahamove để tạo đơn
            var ahamoveResponse = await _ahamoveService.CreateOrderAsync(order);

            // Cập nhật thông tin đơn hàng sang trạng thái đang vận chuyển (StatusId = 3)
            order.AhamoveOrderId = ahamoveResponse.OrderId;
            order.AhamoveStatus = ahamoveResponse.Status;
            order.AhamoveSharedLink = ahamoveResponse.SharedLink;
            order.ActualShippingFee = ahamoveResponse.TotalFee;
            order.OrderStatusId = 3; // 3 = Shipping

            await _context.SaveChangesAsync();

            return new OrderResponse
            {
                Id = order.Id,
                StatusId = order.OrderStatusId,
                StatusName = "Đang vận chuyển",
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
                Note = order.Note,
                DeliveryLatitude = order.DeliveryLatitude,
                DeliveryLongitude = order.DeliveryLongitude,
                AhamoveOrderId = order.AhamoveOrderId,
                AhamoveStatus = order.AhamoveStatus,
                AhamoveSharedLink = order.AhamoveSharedLink,
                ActualShippingFee = order.ActualShippingFee,
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
        }
    }
}
