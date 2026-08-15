// ==========================================================================
// MODULE: WarrantyController.cs
// MỤC ĐÍCH: API Controller xử lý yêu cầu kích hoạt bảo hành, tra cứu hạn bảo hành và tạo yêu cầu hỗ trợ từ Khách hàng.
// ==========================================================================
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

        // ================= LẤY TẤT CẢ GÓI BẢO HÀNH ĐANG KÍCH HOẠT (CHO CLIENT) =================
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetAllWarranties` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllWarranties()
        {
            var warranties = await _context.Warranties
                .Where(w => w.IsActive)
                .OrderBy(w => w.BasePrice)
                .Select(w => new
                {
                    w.Id,
                    w.Code,
                    w.Name,
                    w.Description,
                    w.TermsHtml,
                    w.DurationMonths,
                    w.BasePrice,
                    w.RequiresInspection
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(warranties);
        }

        // ================= LẤY DANH SÁCH BẢO HÀNH & THIẾT BỊ CỦA TÔI =================
        [HttpGet("my-devices")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `GetMyDevices` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetMyDevices()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized(new { message = "Vui lòng đăng nhập." });
            }

            // Lấy danh sách thiết bị từ CustomerDevices
            var devices = await _context.CustomerDevices
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    d.Id,
                    d.ImeiOrSerial,
                    d.ProductName,
                    d.PurchaseDate,
                    d.CreatedAt
                })
                .ToListAsync();

            // Lấy các dòng OrderItem có mua bảo hành của User
            var orderWarranties = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Warranty)
                .Include(oi => oi.CustomerDevice)
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(oi => oi.Order.UserId == userId && oi.WarrantyId.HasValue)
                .OrderByDescending(oi => oi.Order.CreatedAt)
                .Select(oi => new
                {
                    OrderItemId = oi.Id,
                    OrderId = oi.OrderId,
                    OrderDate = oi.Order.CreatedAt,
                    OrderStatusId = oi.Order.OrderStatusId,
                    OrderStatusName = oi.Order.OrderStatus != null ? oi.Order.OrderStatus.Name : "",
                    WarrantyId = oi.WarrantyId,
                    WarrantyName = oi.Warranty != null ? oi.Warranty.Name : "",
                    WarrantyCode = oi.Warranty != null ? oi.Warranty.Code : "",
                    DurationMonths = oi.Warranty != null ? oi.Warranty.DurationMonths : 12,
                    WarrantyPrice = oi.WarrantyPrice,
                    ProductName = oi.CustomerDevice != null ? oi.CustomerDevice.ProductName : (oi.ProductVariant != null ? oi.ProductVariant.Product.Name + " (" + oi.ProductVariant.Name + ")" : "Gói Bảo Hành Độc Lập"),
                    Imei = oi.CustomerDevice != null ? oi.CustomerDevice.ImeiOrSerial : "",
                    CustomerDeviceId = oi.CustomerDeviceId,
                    InspectionStatus = oi.InspectionStatus,
                    IsActivated = oi.CustomerDevice != null && !string.IsNullOrEmpty(oi.CustomerDevice.ImeiOrSerial) && oi.CustomerDevice.ImeiOrSerial != "CHƯA_KÍCH_HOẠT",
                    ExpireDate = oi.Order.CreatedAt.AddMonths(oi.Warranty != null ? oi.Warranty.DurationMonths : 12)
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new
            {
                devices,
                warranties = orderWarranties
            });
        }

        // ================= KÍCH HOẠT / CẬP NHẬT MÃ IMEI CHO GÓI BẢO HÀNH =================
        [HttpPost("activate-imei")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `ActivateImei` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ActivateImei([FromBody] ActivateImeiRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Imei))
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Mã IMEI không được để trống." });
            }

            var cleanImei = request.Imei.Trim();
            if (cleanImei.Length != 15 || !cleanImei.All(char.IsDigit))
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Mã IMEI phải chứa đúng 15 chữ số từ 0-9." });
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized(new { message = "Vui lòng đăng nhập." });
            }

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.CustomerDevice)
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(oi => oi.Id == request.OrderItemId && oi.Order.UserId == userId);

            if (orderItem == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy thông tin đơn hàng bảo hành." });
            }

            if (orderItem.CustomerDevice != null)
            {
                orderItem.CustomerDevice.ImeiOrSerial = cleanImei;
            }
            else
            {
                var prodName = orderItem.ProductVariant != null
                    ? orderItem.ProductVariant.Product.Name + " (" + orderItem.ProductVariant.Name + ")"
                    : "Gói bảo hành mở rộng";

                var device = new CustomerDevice
                {
                    UserId = userId,
                    ImeiOrSerial = cleanImei,
                    ProductName = prodName,
                    VariantId = orderItem.VariantId,
                    PurchaseDate = orderItem.Order.CreatedAt,
                    CreatedAt = DateTime.UtcNow
                };

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.CustomerDevices.Add(device);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                orderItem.CustomerDeviceId = device.Id;
            }

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new
            {
                message = "Kích hoạt mã IMEI thành công! Gói bảo hành đã có hiệu lực.",
                imei = cleanImei
            });
        }

        // LẤY GÓI BẢO HÀNH PHÙ HỢP CHO BIẾN THỂ SẢN PHẨM
        [HttpGet("variants/{variantId}")]
        // [Hàm thực thi nghiệp vụ]: `GetWarrantiesForVariant` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetWarrantiesForVariant(int variantId)
        {
            var variant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == variantId);

            if (variant == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy biến thể sản phẩm." });

            var ancestorCatIds = await GetAncestorCategoryIds(variant.Product.CategoryId);

            var warranties = await _context.Warranties
                .Where(w => w.IsActive)
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                .Where(w => _context.WarrantyPackageRules.Any(r =>
                    r.WarrantyId == w.Id &&
                    (r.ProductId == null || r.ProductId == variant.ProductId) &&
                    (r.CategoryId == null || ancestorCatIds.Contains(r.CategoryId.Value)) &&
                    (r.BrandId == null || r.BrandId == variant.Product.BrandId) &&
                    variant.Price >= r.MinPrice &&
                    (r.MaxPrice == null || variant.Price <= r.MaxPrice)
                ))
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(warranties);
        }

        // ĐẶT MUA LẺ GÓI BẢO HÀNH
        [HttpPost("standalone/checkout")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `StandaloneCheckout` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> StandaloneCheckout([FromBody] StandaloneWarrantyCheckoutRequest request)
        {
            if (request == null)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var warranty = await _context.Warranties.FirstOrDefaultAsync(w => w.Id == request.WarrantyId && w.IsActive);
            if (warranty == null)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Gói bảo hành không hợp lệ hoặc đã bị khóa." });

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện giao dịch." });
            }

            var cleanImei = string.IsNullOrWhiteSpace(request.Imei) ? "CHƯA_KÍCH_HOẠT" : request.Imei.Trim();

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customerDevice = new CustomerDevice
                {
                    UserId = userId,
                    ImeiOrSerial = cleanImei,
                    ProductName = warranty.Name,
                    PurchaseDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                if (request.VariantId > 0)
                {
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    var variant = await _context.ProductVariants.Include(pv => pv.Product).FirstOrDefaultAsync(pv => pv.Id == request.VariantId);
                    if (variant != null)
                    {
                        customerDevice.ProductName = variant.Product.Name + " (" + variant.Name + ")";
                        customerDevice.VariantId = variant.Id;
                    }
                }

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.CustomerDevices.Add(customerDevice);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                var newOrder = new Order
                {
                    UserId = userId,
                    ReceiverName = request.ReceiverName ?? "Khách hàng",
                    ReceiverPhone = request.ReceiverPhone ?? "0900000000",
                    ShippingAddressLine = "Đăng ký mua gói bảo hành: " + warranty.Name + " (IMEI: " + cleanImei + ")",
                    ShippingWard = "N/A",
                    ShippingProvince = "N/A",
                    TotalPrice = warranty.BasePrice,
                    OrderStatusId = 1, // Chờ thanh toán
                    CreatedAt = DateTime.UtcNow,
                    PaymentMethod = "Stripe",
                    ShippingCarrier = "Gói Bảo Hành Trực Tuyến",
                    Note = "Đơn đăng ký gói bảo hành mở rộng: " + warranty.Name,
                    ActualShippingFee = 0
                };
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Orders.Add(newOrder);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // Lấy 1 ProductVariant mặc định hoặc gán VariantId
                int varId = request.VariantId > 0 ? request.VariantId : (await _context.ProductVariants.Select(pv => pv.Id).FirstOrDefaultAsync());

                var orderItem = new OrderItem
                {
                    OrderId = newOrder.Id,
                    VariantId = varId,
                    Quantity = 1,
                    PriceAtPurchase = 0,
                    WarrantyId = warranty.Id,
                    WarrantyPrice = warranty.BasePrice,
                    CustomerDeviceId = customerDevice.Id,
                    InspectionStatus = warranty.RequiresInspection ? "WAITING_CHECK" : "PASSED"
                };
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.OrderItems.Add(orderItem);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new
                {
                    message = "Đăng ký mua gói bảo hành thành công!",
                    orderId = newOrder.Id,
                    receiverPhone = newOrder.ReceiverPhone
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Lỗi trong quá trình tạo đơn hàng: " + ex.Message });
            }
        }

        private async Task<List<int>> GetAncestorCategoryIds(int categoryId)
        {
            var list = new List<int> { categoryId };
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var current = await _context.Categories.FindAsync(categoryId);
            while (current != null && current.ParentId.HasValue)
            {
                list.Add(current.ParentId.Value);
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                current = await _context.Categories.FindAsync(current.ParentId.Value);
            }
            return list;
        }
    }

    public class StandaloneWarrantyCheckoutRequest
    {
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? Imei { get; set; }
        public int WarrantyId { get; set; }
        public int VariantId { get; set; }
    }

    public class ActivateImeiRequest
    {
        public int OrderItemId { get; set; }
        public string Imei { get; set; }
    }
}

