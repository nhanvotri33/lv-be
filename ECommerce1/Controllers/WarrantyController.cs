using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarrantyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarrantyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= LẤY GÓI BẢO HÀNH PHÙ HỢP CHO BIẾN THỂ SẢN PHẨM =================
        // Logic xử lý:
        // 1. Tìm biến thể sản phẩm (ProductVariant) cùng với thông tin sản phẩm cha (Product).
        // 2. Lấy toàn bộ cây danh mục cha (Category Ancestors) để đối chiếu quy tắc phân cấp danh mục.
        // 3. Truy vấn các gói bảo hành có cấu hình quy tắc (WarrantyPackageRules) khớp với:
        //    - ProductId trùng khớp, HOẶC CategoryId nằm trong danh mục tổ tiên, HOẶC BrandId trùng khớp, HOẶC áp dụng chung cho mọi sản phẩm (tất cả các FK đều NULL).
        //    - Giá bán hiện tại của biến thể nằm trong khoảng [MinPrice, MaxPrice] của quy tắc.
        [HttpGet("variants/{variantId}")]
        public async Task<IActionResult> GetWarrantiesForVariant(int variantId)
        {
            var variant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == variantId);

            if (variant == null)
                return NotFound(new { message = "Không tìm thấy biến thể sản phẩm." });

            // Lấy danh sách ID danh mục tổ tiên
            var ancestorCatIds = await GetAncestorCategoryIds(variant.Product.CategoryId);

            // ================= LUỒNG TRUY VẤN GÓI BẢO HÀNH THỎA MÃN QUY TẮC RÀNG BUỘC =================
            // Hệ thống thực hiện truy vấn các gói bảo hành thỏa mãn đồng thời các điều kiện sau:
            // 1. Gói bảo hành phải đang ở trạng thái kích hoạt (IsActive = true).
            // 2. Gói bảo hành đó phải có ít nhất một quy tắc (WarrantyPackageRules) khớp với sản phẩm hiện tại:
            //    - Ràng buộc Hãng (BrandId): Nếu r.BrandId bằng NULL (không chọn) -> Áp dụng cho mọi hãng sản xuất. Nếu có giá trị -> bắt buộc trùng với BrandId của sản phẩm.
            //    - Ràng buộc Danh mục (CategoryId): Nếu r.CategoryId bằng NULL (không chọn) -> Áp dụng cho mọi danh mục. Nếu có giá trị -> bắt buộc danh mục đó hoặc danh mục cha của nó (ancestorCatIds) trùng khớp.
            //    - Ràng buộc Sản phẩm (ProductId): Nếu r.ProductId bằng NULL (không chọn) -> Áp dụng cho mọi sản phẩm. Nếu có giá trị -> chỉ áp dụng riêng cho sản phẩm đó (ví dụ: chỉ cho S24+).
            //    - Ràng buộc Tầm giá máy (MinPrice & MaxPrice): Giá bán hiện tại của biến thể (variant.Price) phải nằm trong khoảng [r.MinPrice, r.MaxPrice].
            //      + Nếu không nhập MaxPrice (NULL) -> không giới hạn giá tối đa.
            //      + Nếu MinPrice bằng 0 -> áp dụng từ giá trị nhỏ nhất.
            var warranties = await _context.Warranties
                .Where(w => w.IsActive)
                .Where(w => _context.WarrantyPackageRules.Any(r =>
                    r.WarrantyId == w.Id &&
                    (r.ProductId == null || r.ProductId == variant.ProductId) &&
                    (r.CategoryId == null || ancestorCatIds.Contains(r.CategoryId.Value)) &&
                    (r.BrandId == null || r.BrandId == variant.Product.BrandId) &&
                    variant.Price >= r.MinPrice &&
                    (r.MaxPrice == null || variant.Price <= r.MaxPrice)
                ))
                .ToListAsync();

            return Ok(warranties);
        }

        // ================= ĐẶT MUA LẺ GÓI BẢO HÀNH (MÁY CŨ - CẦN THẨM ĐỊNH) =================
        // Logic xử lý:
        // 1. Kiểm tra sự tồn tại của gói bảo hành và biến thể máy tương ứng.
        // 2. Lưu vết thiết bị cũ của khách vào bảng `CustomerDevices` (lưu IMEI, tên sản phẩm và ngày kích hoạt).
        // 3. Tạo một hóa đơn mới (`Order`) ở trạng thái chờ thanh toán (Pending - OrderStatusId = 1).
        // 4. Tạo chi tiết đơn hàng (`OrderItem`) liên kết thiết bị trên, gán `PriceAtPurchase = 0` (vì chỉ mua lẻ bảo hành) và set `InspectionStatus = WAITING_CHECK` để yêu cầu kỹ thuật viên thẩm định máy cũ tại quầy.
        [HttpPost("standalone/checkout")]
        [Authorize]
        public async Task<IActionResult> StandaloneCheckout([FromBody] StandaloneWarrantyCheckoutRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            var warranty = await _context.Warranties.FirstOrDefaultAsync(w => w.Id == request.WarrantyId && w.IsActive);
            if (warranty == null)
                return BadRequest(new { message = "Gói bảo hành không hợp lệ hoặc đã bị khóa." });

            var variant = await _context.ProductVariants.Include(pv => pv.Product).FirstOrDefaultAsync(pv => pv.Id == request.VariantId);
            if (variant == null)
                return BadRequest(new { message = "Biến thể sản phẩm máy cũ không hợp lệ." });

            // Đọc UserId từ JWT Token (yêu cầu người dùng phải đăng nhập để đảm bảo tính nhất quán của hệ thống)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện giao dịch." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo bản ghi thông tin thiết bị cũ của khách
                var customerDevice = new CustomerDevice
                {
                    UserId = userId,
                    ImeiOrSerial = request.Imei,
                    ProductName = variant.Product.Name + " (" + variant.Name + ")",
                    VariantId = variant.Id,
                    PurchaseDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CustomerDevices.Add(customerDevice);
                await _context.SaveChangesAsync();

                // 2. Tạo đơn hàng mới ở trạng thái chờ thanh toán
                var newOrder = new Order
                {
                    UserId = userId,
                    ReceiverName = request.ReceiverName,
                    ReceiverPhone = request.ReceiverPhone,
                    ShippingAddressLine = "Thẩm định & Nhận gói bảo hành lẻ tại cửa hàng. IMEI: " + request.Imei,
                    ShippingWard = "N/A",
                    ShippingProvince = "N/A",
                    TotalPrice = warranty.BasePrice,
                    OrderStatusId = 1, // Chờ thanh toán
                    CreatedAt = DateTime.UtcNow,
                    PaymentMethod = "Stripe",
                    ShippingCarrier = "Nhận tại cửa hàng",
                    Note = "Đơn đặt mua lẻ gói bảo hành mở rộng: " + warranty.Name,
                    ActualShippingFee = 0
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // 3. Tạo chi tiết đơn hàng
                var orderItem = new OrderItem
                {
                    OrderId = newOrder.Id,
                    VariantId = variant.Id,
                    Quantity = 1,
                    PriceAtPurchase = 0, // Tiền máy bằng 0 (khách chỉ mua gói bảo hành lẻ)
                    WarrantyId = warranty.Id,
                    WarrantyPrice = warranty.BasePrice,
                    CustomerDeviceId = customerDevice.Id,
                    InspectionStatus = "WAITING_CHECK" // BẮT BUỘC THẨM ĐỊNH TẠI CỬA HÀNG
                };
                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Đặt mua gói bảo hành lẻ thành công. Vui lòng mang thiết bị đến cửa hàng để thẩm định.",
                    orderId = newOrder.Id,
                    receiverPhone = newOrder.ReceiverPhone
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi trong quá trình tạo đơn hàng: " + ex.Message });
            }
        }

        // Hàm đệ quy lấy toàn bộ tổ tiên danh mục
        private async Task<List<int>> GetAncestorCategoryIds(int categoryId)
        {
            var list = new List<int> { categoryId };
            var current = await _context.Categories.FindAsync(categoryId);
            while (current != null && current.ParentId.HasValue)
            {
                list.Add(current.ParentId.Value);
                current = await _context.Categories.FindAsync(current.ParentId.Value);
            }
            return list;
        }
    }

    public class StandaloneWarrantyCheckoutRequest
    {
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string Imei { get; set; }
        public int WarrantyId { get; set; }
        public int VariantId { get; set; }
    }
}
