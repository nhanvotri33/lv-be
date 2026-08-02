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
        private readonly IEmailService _emailService;

        public OrderService(ApplicationDbContext context, 
            IEnumerable<ECommerce1.Services.Payment.IPaymentProvider> paymentProviders,
            IAhamoveService ahamoveService,
            IEmailService emailService)
        {
            _context = context;
            _paymentProviders = paymentProviders;
            _ahamoveService = ahamoveService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Warranty)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.CustomerDevice)
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
                    ShippingCarrier = o.ShippingCarrier,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        Id = oi.Id,
                        VariantId = oi.VariantId,
                        ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "Sản phẩm không rõ",
                        VariantName = oi.ProductVariant != null ? oi.ProductVariant.Name : "Biến thể không rõ",
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase,
                        WarrantyId = oi.WarrantyId,
                        WarrantyName = oi.Warranty != null ? oi.Warranty.Name : null,
                        WarrantyPrice = oi.WarrantyPrice,
                        CustomerDeviceId = oi.CustomerDeviceId,
                        ImeiOrSerial = oi.CustomerDevice != null ? oi.CustomerDevice.ImeiOrSerial : null,
                        CustomerDeviceProductName = oi.CustomerDevice != null ? oi.CustomerDevice.ProductName : null,
                        InspectionStatus = oi.InspectionStatus
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
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Warranty)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.CustomerDevice)
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
                    ShippingCarrier = o.ShippingCarrier,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        Id = oi.Id,
                        VariantId = oi.VariantId,
                        ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "Sản phẩm không rõ",
                        VariantName = oi.ProductVariant != null ? oi.ProductVariant.Name : "Biến thể không rõ",
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase,
                        WarrantyId = oi.WarrantyId,
                        WarrantyName = oi.Warranty != null ? oi.Warranty.Name : null,
                        WarrantyPrice = oi.WarrantyPrice,
                        CustomerDeviceId = oi.CustomerDeviceId,
                        ImeiOrSerial = oi.CustomerDevice != null ? oi.CustomerDevice.ImeiOrSerial : null,
                        CustomerDeviceProductName = oi.CustomerDevice != null ? oi.CustomerDevice.ProductName : null,
                        InspectionStatus = oi.InspectionStatus
                    }).ToList()
                })
                .ToListAsync();
        }

        /// <summary>
        /// LUỒNG HOẠT ĐỘNG ĐẶT HÀNG (CHECKOUT) VÀ MUA KÈM PHỤ KIỆN:
        /// 1. Mở Transaction Serializable tránh bán đè khi cùng tranh chấp tồn kho.
        /// 2. Check AvailableStock (TotalStock - ReservedStock). Nếu thiếu sẽ báo lỗi ngay.
        /// 3. Khuyến mãi mua kèm (Self-Reference ParentCartItemId):
        ///    - Phụ kiện (IsAddon = true) chỉ giảm nếu có Máy chính trong giỏ hàng (ParentCartItemId != null).
        ///    - Giới hạn số lượng mua kèm: Qty Phụ kiện <= Qty Máy chính * MaxQuantityAllowed.
        ///    - Giảm giá cố định theo campaign, không cộng dồn lũy tiến khi mua nhiều phụ kiện.
        /// 4. Tạo Order & OrderItems (Lưu ParentOrderItemId để giữ liên kết combo).
        /// 5. Tăng ReservedStock để giữ hàng tạm thời. Xóa CartItems và Commit.
        /// </summary>
        public async Task<object> CheckoutAsync(Guid userId, CheckoutRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // 1. Lấy giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Warranty)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    throw new ArgumentException("Giỏ hàng của bạn đang trống.");

                // 2. Khóa dòng ProductVariants với UPDLOCK để tránh Deadlock và Race Condition khi nhiều người cùng đặt hàng
                var variantIds = cart.CartItems.Select(ci => ci.VariantId).Distinct().ToList();
                foreach (var vId in variantIds)
                {
                    await _context.ProductVariants
                        .FromSqlRaw("SELECT * FROM ProductVariants WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0}", vId)
                        .FirstOrDefaultAsync();
                }

                // 3. Kiểm tra tồn kho trước khi đặt
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

                    if (item.AppliedCampaignId.HasValue && item.IsAddon)
                    {
                        var campaign = await _context.PromotionCampaigns
                            .Include(c => c.MainProductRules)
                            .FirstOrDefaultAsync(c => c.Id == item.AppliedCampaignId.Value && c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow);

                        if (campaign != null)
                        {
                            // Tìm sản phẩm chính trong giỏ hàng
                            CartItem parentItem = null;
                            if (item.ParentCartItemId.HasValue)
                            {
                                parentItem = cartItemsList.FirstOrDefault(ci => ci.Id == item.ParentCartItemId && !ci.IsAddon);
                            }
                            
                            if (parentItem == null)
                            {
                                // Tự động tìm sản phẩm chính thỏa mãn điều kiện chiến dịch (Logic AND trong dòng, OR giữa các dòng)
                                foreach (var ci in cartItemsList.Where(ci => !ci.IsAddon))
                                {
                                    if (campaign.MainProductRules == null || !campaign.MainProductRules.Any())
                                    {
                                        parentItem = ci;
                                        break;
                                    }

                                    var ancestorCatIds = await GetAncestorCategoryIds(ci.ProductVariant.Product.CategoryId);
                                    foreach (var rule in campaign.MainProductRules)
                                    {
                                        bool matchesRule = true;
                                        if (rule.ProductId.HasValue && rule.ProductId.Value != ci.ProductVariant.ProductId)
                                            matchesRule = false;
                                        if (matchesRule && rule.CategoryId.HasValue && !ancestorCatIds.Contains(rule.CategoryId.Value))
                                            matchesRule = false;
                                        if (matchesRule && rule.BrandId.HasValue && rule.BrandId.Value != ci.ProductVariant.Product.BrandId)
                                            matchesRule = false;

                                        if (matchesRule)
                                        {
                                            parentItem = ci;
                                            break;
                                        }
                                    }

                                    if (parentItem != null) break;
                                }
                            }

                            if (parentItem != null)
                            {
                                item.ParentCartItemId = parentItem.Id; // Cập nhật liên kết

                                // Ràng buộc số lượng sản phẩm phụ theo tỷ lệ sản phẩm chính
                                int allowedMaxQty = parentItem.Quantity * campaign.MaxQuantityAllowed;
                                if (item.Quantity > allowedMaxQty)
                                {
                                    string pName = item.ProductVariant?.Product?.Name ?? item.ProductVariant?.Name ?? "Sản phẩm";
                                    throw new ArgumentException($"Sản phẩm phụ '{pName}' vượt quá số lượng mua kèm cho phép ({allowedMaxQty} sản phẩm cho {parentItem.Quantity} sản phẩm chính).");
                                }

                                if (campaign.DiscountType == "Percentage")
                                {
                                    decimal calculatedDiscount = price * (campaign.DiscountValue / 100m);
                                    if (campaign.MaxDiscountAmount.HasValue && calculatedDiscount > campaign.MaxDiscountAmount.Value)
                                    {
                                        calculatedDiscount = campaign.MaxDiscountAmount.Value;
                                    }
                                    price = Math.Max(0, price - calculatedDiscount);
                                }
                                else if (campaign.DiscountType == "FixedAmount")
                                {
                                    price = Math.Max(0, price - campaign.DiscountValue);
                                }
                                else if (campaign.DiscountType == "FixedPrice")
                                {
                                    price = campaign.DiscountValue;
                                }
                            }
                            else
                            {
                                // Không có sản phẩm chính tương ứng -> Hủy khuyến mãi
                                item.AppliedCampaignId = null;
                                item.ParentCartItemId = null;
                                item.IsAddon = false;
                            }
                        }
                    }
                    calculatedPrices[item.Id] = price;
                    subTotal += price * item.Quantity;
                    // Cộng thêm giá gói bảo hành đi kèm (nếu có)
                    if (item.WarrantyId.HasValue && item.Warranty != null)
                    {
                        subTotal += item.Warranty.BasePrice * item.Quantity;
                    }
                }

                decimal discountValue = 0;
                Promotion appliedPromotion = null;

                // 4. Áp dụng mã giảm giá (Khóa bảng Promotion với UPDLOCK để ngăn quá UsageLimit khi đồng thời checkout)
                if (!string.IsNullOrEmpty(request.PromotionCode))
                {
                    appliedPromotion = await _context.Promotions
                        .FromSqlRaw("SELECT * FROM Promotions WITH (UPDLOCK, HOLDLOCK) WHERE Code = {0} AND IsActive = 1", request.PromotionCode)
                        .FirstOrDefaultAsync();

                    if (appliedPromotion == null)
                        throw new ArgumentException("Mã giảm giá không tồn tại hoặc đã bị khóa.");

                    if (DateTime.UtcNow < appliedPromotion.StartDate || DateTime.UtcNow > appliedPromotion.EndDate)
                        throw new ArgumentException("Mã giảm giá đã hết hạn hoặc chưa tới thời gian sử dụng.");

                    if (appliedPromotion.MinOrderAmount.HasValue && subTotal < appliedPromotion.MinOrderAmount.Value)
                        throw new ArgumentException($"Đơn hàng chưa đạt giá trị tối thiểu {appliedPromotion.MinOrderAmount.Value:N0}đ để áp dụng mã này.");

                    // Kiểm tra User đã dùng mã này bao nhiêu lần 
                    int maxAllowed = appliedPromotion.MaxPerUser.HasValue && appliedPromotion.MaxPerUser.Value > 0 ? appliedPromotion.MaxPerUser.Value : 1;
                    int userUsageCount = await _context.PromotionUsages.CountAsync(pu => pu.PromotionId == appliedPromotion.Id && pu.UserId == userId);
                    if (userUsageCount >= maxAllowed)
                        throw new ArgumentException($"Bạn đã sử dụng mã giảm giá này tối đa {maxAllowed} lần cho phép.");

                    // Kiểm tra giới hạn số lượng mã đã phát hành
                    if (appliedPromotion.UsageLimit > 0 && appliedPromotion.UsedCount >= appliedPromotion.UsageLimit)
                        throw new ArgumentException("Mã giảm giá này đã hết lượt sử dụng.");

                    if (appliedPromotion.DiscountType.ToUpper() == "PERCENTAGE")
                    {
                        discountValue = subTotal * (appliedPromotion.DiscountValue / 100);
                        if (appliedPromotion.MaxDiscountAmount.HasValue && discountValue > appliedPromotion.MaxDiscountAmount.Value)
                        {
                            discountValue = appliedPromotion.MaxDiscountAmount.Value;
                        }
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

                // [LUỒNG TRỪ ĐIỂM TIÊU DÙNG ĐỂ GIẢM GIÁ HÓA ĐƠN]:
                // - Kiểm tra xem khách có yêu cầu quy đổi điểm để giảm giá hay không.
                if (request.PointsToRedeem > 0)
                {
                    // 1. Kiểm tra ví điểm hiện có của khách hàng xem có đủ số điểm muốn đổi hay không.
                    if (user.RewardPoints < request.PointsToRedeem)
                        throw new ArgumentException("Số điểm tích lũy của bạn không đủ.");

                    pointsRedeemed = request.PointsToRedeem;
                    discountFromPoints = pointsRedeemed; // Quy đổi 1 điểm = 1 VNĐ
                    
                    // 2. Chặn quy đổi vượt quá tổng tiền hóa đơn cần thanh toán (tránh hóa đơn bị âm tiền).
                    if (discountFromPoints > priceBeforePoints)
                    {
                        discountFromPoints = priceBeforePoints;
                        pointsRedeemed = (int)discountFromPoints; // Cập nhật lại số điểm bị trừ thực tế khớp với tiền hàng
                    }
                    priceBeforePoints -= discountFromPoints; // Giảm trừ trực tiếp số tiền quy đổi vào hóa đơn
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

                if (deliveryLat.HasValue && deliveryLng.HasValue && !string.IsNullOrEmpty(request.ShippingCarrier) && request.ShippingCarrier.Contains("Ahamove", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var destAddress = $"{shippingAddressLine}, {shippingWard}, {shippingProvince}";
                        string serviceId = "SGN-BIKE"; // Mặc định Siêu Tốc

                        if (request.ShippingCarrier.Contains("4H", StringComparison.OrdinalIgnoreCase)) serviceId = "SGN-POOL";
                        else if (request.ShippingCarrier.Contains("Tiết Kiệm", StringComparison.OrdinalIgnoreCase)) serviceId = "SGN-EXPRESS";

                        shippingFee = await _ahamoveService.EstimateFeeAsync(deliveryLat.Value, deliveryLng.Value, destAddress, serviceId);
                        isAhamoveCalculated = true;
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi nếu cần thiết, ở đây chúng ta sẽ fallback về tính phí mặc định để không block khách thanh toán
                        Console.WriteLine($"Lỗi gọi API Ahamove: {ex.Message}. Sử dụng cách tính phí mặc định làm phương án dự phòng.");
                    }
                }

                // Fallback nếu không có tọa độ, API Ahamove gặp sự cố, hoặc khách chọn Giao Hàng Tiêu Chuẩn
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

                 // =========================================================================
                 // [TÍCH LŨY ĐIỂM THƯỞNG - BACK-END]
                 // - Hệ số: 0.002m tương ứng với 0.2% giá trị đơn hàng thanh toán cuối cùng (finalPrice).
                 // - Ví dụ: đơn hàng 10.000.000đ thì khách nhận được: 10.000.000 * 0.002 = 20.000 điểm.
                 // - Nếu muốn đổi sang 20%, đổi 0.002m thành 0.2m ở đây.
                 // =========================================================================
                 // Tích lũy điểm thưởng: 0.2% trên số tiền thanh toán cuối cùng
                 int pointsEarned = (int)(finalPrice * 0.002m);

                if (pointsRedeemed > 0)
                {
                    // Khấu trừ trực tiếp số điểm thưởng đã quy đổi khỏi ví của khách hàng trong Database
                    user.RewardPoints -= pointsRedeemed;
                }

                // =========================================================================
                // [TẠO ĐƠN HÀNG - BACK-END]
                // - Mã đơn hàng (Id) là khóa chính tự tăng (IDENTITY(1,1)) trong SQL Server.
                // - Khi gọi SaveChangesAsync(), cơ sở dữ liệu sẽ tự sinh Id cho đơn hàng này.
                // =========================================================================
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
                    PaymentMethod = request.PaymentMethod ?? "COD",
                    ShippingCarrier = request.ShippingCarrier ?? (isAhamoveCalculated ? "Ahamove (Giao Siêu Tốc)" : "Giao Hàng Tiêu Chuẩn"),
                    ActualShippingFee = shippingFee
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // Lưu để lấy Order.Id

                // 7. Tạo OrderItems và trừ Tồn kho giữ chỗ (ReservedStock)
                var orderItemMap = new System.Collections.Generic.Dictionary<int, OrderItem>(); // CartItemId -> OrderItem
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
                        AppliedCampaignId = item.AppliedCampaignId,
                        CampaignDiscountAmount = comboDiscountAmt,
                        IsAddon = item.IsAddon,
                        WarrantyId = item.WarrantyId,
                        WarrantyPrice = item.Warranty != null ? item.Warranty.BasePrice : 0,
                        InspectionStatus = "NOT_REQUIRED" // Đơn mua kèm máy tại shop mặc định đạt chuẩn
                    };
                    _context.OrderItems.Add(orderItem);
                    orderItemMap[item.Id] = orderItem;

                    // Quan trọng: Tăng ReservedStock lên để giữ hàng cho khách này
                    item.ProductVariant.ReservedStock += item.Quantity;
                }

                // Cập nhật ParentOrderItemId
                foreach (var item in cart.CartItems)
                {
                    if (item.IsAddon && item.ParentCartItemId.HasValue && orderItemMap.ContainsKey(item.ParentCartItemId.Value))
                    {
                        orderItemMap[item.Id].ParentOrderItem = orderItemMap[item.ParentCartItemId.Value];
                    }
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

                // GỬI EMAIL XÁC NHẬN ĐƠN HÀNG
                try
                {
                    string recipientEmail = !string.IsNullOrWhiteSpace(request.Email) ? request.Email.Trim() : user.Email;

                    if (!string.IsNullOrWhiteSpace(recipientEmail))
                    {
                        string itemsHtml = "";
                        foreach (var item in cartItemsList)
                        {
                            string prodName = item.ProductVariant?.Product?.Name ?? "Sản phẩm";
                            string variantName = item.ProductVariant?.Name ?? "Mặc định";
                            decimal price = calculatedPrices.ContainsKey(item.Id) ? calculatedPrices[item.Id] : item.ProductVariant.Price;

                            itemsHtml += $@"
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{prodName} ({variantName})</td>
                                    <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                                    <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{price:N0}đ</td>
                                </tr>";
                        }

                        string emailBody = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
                                <h2 style='color: #2b6cb0; text-align: center;'>Xác Nhận Đơn Hàng Thành Công</h2>
                                <p>Chào <b>{receiverName}</b>,</p>
                                <p>Cảm ơn bạn đã mua sắm tại <b>PhoneStore</b>. Đơn hàng của bạn đã được tiếp nhận thành công!</p>
                                
                                <h3 style='border-bottom: 2px solid #2b6cb0; padding-bottom: 5px;'>Thông tin đơn hàng #{newOrder.Id}</h3>
                                <p><b>Người nhận:</b> {receiverName} ({receiverPhone})</p>
                                <p><b>Địa chỉ nhận:</b> {shippingAddressLine}, {shippingWard}, {shippingProvince}</p>
                                <p><b>Phương thức thanh toán:</b> {newOrder.PaymentMethod}</p>
                                
                                <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                    <thead>
                                        <tr style='background: #f7fafc;'>
                                            <th style='padding: 8px; border: 1px solid #ddd; text-align: left;'>Sản phẩm</th>
                                            <th style='padding: 8px; border: 1px solid #ddd; text-align: center;'>Số lượng</th>
                                            <th style='padding: 8px; border: 1px solid #ddd; text-align: right;'>Đơn giá</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {itemsHtml}
                                    </tbody>
                                </table>
                                
                                <h3 style='text-align: right; color: #e53e3e;'>Tổng tiền thanh toán: {finalPrice:N0}đ</h3>
                                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'/>
                                <p style='font-size: 12px; color: #718096; text-align: center;'>Mọi thắc mắc vui lòng liên hệ hotline 1900xxxx. Xin cảm ơn quý khách!</p>
                            </div>";

                        _ = _emailService.SendEmailAsync(recipientEmail, $"[PhoneStore] Xác nhận đơn hàng #{newOrder.Id} thành công", emailBody);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LỖI GỬI EMAIL]: {ex.Message}");
                }
                // ===============================================================

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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Order order = null;

                if (userId.HasValue)
                {
                    order = await _context.Orders
                        .FromSqlRaw("SELECT * FROM Orders WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0} AND UserId = {1}", id, userId.Value)
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.ProductVariant)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    if (string.IsNullOrEmpty(phoneNumber))
                        throw new UnauthorizedAccessException("Bạn cần đăng nhập hoặc cung cấp số điện thoại nhận hàng để hủy đơn hàng.");

                    order = await _context.Orders
                        .FromSqlRaw("SELECT * FROM Orders WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0} AND ReceiverPhone = {1}", id, phoneNumber.Trim())
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.ProductVariant)
                        .FirstOrDefaultAsync();
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
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// LUỒNG ĐỒNG BỘ KHO KHI THAY ĐỔI TRẠNG THÁI ĐƠN HÀNG:
        /// - Chờ duyệt (1) -> Confirmed/Shipping/Delivered (2,3,4): Trừ kho vật lý TotalStock & giải phóng ReservedStock.
        /// - Đang giao/Duyệt (2,3) -> Hủy/Thất bại/Hoàn tiền (5,6,7): Hoàn trả kho vật lý TotalStock.
        /// - Chờ duyệt (1) -> Hủy/Thất bại/Hoàn tiền (5,6,7): Chỉ giải phóng ReservedStock.
        /// - Tính tổng (Sum) kho của tất cả biến thể để cập nhật đồng bộ lên bảng cha Products.
        /// </summary>
        public async Task UpdateOrderStatusAsync(int id, int newStatusId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Khóa hàng đơn hàng bằng UPDLOCK để tránh race condition khi Webhook & Admin cập nhật cùng lúc
                var order = await _context.Orders
                    .FromSqlRaw("SELECT * FROM Orders WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0}", id)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                    .FirstOrDefaultAsync();

                if (order == null)
                    throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

                var statusExists = await _context.OrderStatuses.AnyAsync(s => s.Id == newStatusId);
                if (!statusExists)
                    throw new ArgumentException("Trạng thái đơn hàng không hợp lệ.");

                int oldStatusId = order.OrderStatusId;
                if (oldStatusId == newStatusId)
                {
                    await transaction.CommitAsync();
                    return; // Trạng thái không đổi
                }

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

                // Xử lý cộng điểm tích lũy khi hoàn thành đơn (OrderStatusId = 4 - Completed)
                // - RewardPoints: Điểm dùng để trừ tiền mua hàng lần sau.
                // - AccumulatedPoints: Điểm tích lũy trọn đời chỉ tăng, không giảm khi đổi quà, dùng xét Hạng thành viên (Đồng/Bạc/Vàng).
                if (newStatusId == 4 && oldStatusId != 4)
                {
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        // Cộng điểm thưởng tích lũy của đơn hàng (đã được tính bằng 0.2% ở bước đặt hàng) vào tài khoản thành viên
                        user.RewardPoints += order.PointsEarned;
                        // Cộng điểm tích lũy trọn đời xét hạng của đơn hàng vào tài khoản thành viên
                        user.AccumulatedPoints += order.PointsEarned;
                    }
                }

                // Xử lý hoàn điểm khi hủy đơn hoặc hoàn tiền
                // TRƯỜNG HỢP 1: Đơn hàng ở trạng thái đang xử lý (1, 2, 3) bị hủy hoặc thất bại (5, 6, 7).
                // - Đơn hàng này chưa bao giờ hoàn thành (chưa đạt trạng thái 4) nên khách chưa được cộng điểm thưởng của đơn này.
                // - Hệ thống chỉ cần hoàn trả lại số điểm cũ mà khách đã tiêu dùng (PointsRedeemed) khi thanh toán đơn hàng này.
                if ((newStatusId == 5 || newStatusId == 6 || newStatusId == 7) && (oldStatusId == 1 || oldStatusId == 2 || oldStatusId == 3))
                {
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null && order.PointsRedeemed > 0)
                    {
                        user.RewardPoints += order.PointsRedeemed;
                    }
                }
                // TRƯỜNG HỢP 2: Đơn hàng đã giao thành công (4) nhưng sau đó bị đổi trả/hoàn tiền (7)
                // - Vì đơn hàng đã hoàn thành trước đó nên khách đã được cộng cả điểm thưởng (RewardPoints) và điểm xét hạng (AccumulatedPoints).
                // - Hệ thống cần:
                //   1. Thu hồi lại số điểm thưởng mới nhận từ đơn này.
                //   2. Thu hồi lại số điểm tích lũy xét hạng mới nhận từ đơn này.
                //   3. Hoàn trả lại số điểm cũ mà khách đã tiêu dùng để thanh toán đơn này.
                else if (oldStatusId == 4 && newStatusId == 7)
                {
                    // Thu hồi điểm tích lũy và hoàn trả điểm đã tiêu dùng (Không hoàn lại kho tồn máy mới)
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        // Thu hồi lại số điểm thưởng tích lũy (RewardPoints) đã nhận trước đó khi đơn bị hủy
                        user.RewardPoints -= order.PointsEarned;
                        if (user.RewardPoints < 0) user.RewardPoints = 0;

                        // Thu hồi lại số điểm tích lũy trọn đời (AccumulatedPoints) đã nhận trước đó khi đơn bị hủy
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
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResponse> TrackOrderAsync(int id, string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                throw new ArgumentException("Vui lòng cung cấp số điện thoại.");

            // LOGIC TRA CỨU KHÁCH VÃNG LAI: 
            // 1. SELECT thông tin từ bảng Orders kết hợp JOIN các bảng OrderItems, ProductVariants, Products, Promotion, OrderStatus để có đầy đủ thông tin hiển thị timeline và chi tiết.
            // 2. Điểm quan trọng: Lọc chính xác theo mã hóa đơn (Id) và số điện thoại nhận hàng của khách (ReceiverPhone) để tránh rò rỉ thông tin đơn hàng của khách hàng khác.
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Warranty)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.CustomerDevice)
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
                ShippingCarrier = order.ShippingCarrier,
                Items = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "Sản phẩm không rõ",
                    VariantName = oi.ProductVariant != null ? oi.ProductVariant.Name : "Biến thể không rõ",
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    WarrantyId = oi.WarrantyId,
                    WarrantyName = oi.Warranty != null ? oi.Warranty.Name : null,
                    WarrantyPrice = oi.WarrantyPrice,
                    CustomerDeviceId = oi.CustomerDeviceId,
                    ImeiOrSerial = oi.CustomerDevice != null ? oi.CustomerDevice.ImeiOrSerial : null,
                    CustomerDeviceProductName = oi.CustomerDevice != null ? oi.CustomerDevice.ProductName : null,
                    InspectionStatus = oi.InspectionStatus
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

            // Xác định Service ID của Ahamove từ tên Đơn vị vận chuyển được chọn
            string serviceId = "SGN-BIKE"; // Mặc định Siêu Tốc

            if (!string.IsNullOrEmpty(order.ShippingCarrier))
            {
                if (order.ShippingCarrier.Contains("4H", StringComparison.OrdinalIgnoreCase)) serviceId = "SGN-POOL";
                else if (order.ShippingCarrier.Contains("Tiết Kiệm", StringComparison.OrdinalIgnoreCase)) serviceId = "SGN-EXPRESS";
                else if (order.ShippingCarrier.Contains("Siêu Tốc", StringComparison.OrdinalIgnoreCase)) serviceId = "SGN-BIKE";
            }

            // Gọi dịch vụ Ahamove để tạo đơn
            var ahamoveResponse = await _ahamoveService.CreateOrderAsync(order, serviceId);

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
                ShippingCarrier = order.ShippingCarrier,
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

        private async Task<HashSet<int>> GetAncestorCategoryIds(int categoryId)
        {
            var result = new HashSet<int> { categoryId };
            var current = await _context.Categories.FindAsync(categoryId);

            while (current?.ParentId != null)
            {
                result.Add(current.ParentId.Value);
                current = await _context.Categories.FindAsync(current.ParentId.Value);
            }

            return result;
        }
    }
}
