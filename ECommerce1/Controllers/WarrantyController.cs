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

            return Ok(warranties);
        }

        // ================= LẤY DANH SÁCH BẢO HÀNH & THIẾT BỊ CỦA TÔI =================
        [HttpGet("my-devices")]
        [Authorize]
        public async Task<IActionResult> GetMyDevices()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
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

            return Ok(new
            {
                devices,
                warranties = orderWarranties
            });
        }

        // ================= KÍCH HOẠT / CẬP NHẬT MÃ IMEI CHO GÓI BẢO HÀNH =================
        [HttpPost("activate-imei")]
        [Authorize]
        public async Task<IActionResult> ActivateImei([FromBody] ActivateImeiRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Imei))
            {
                return BadRequest(new { message = "Mã IMEI không được để trống." });
            }

            var cleanImei = request.Imei.Trim();
            if (cleanImei.Length != 15 || !cleanImei.All(char.IsDigit))
            {
                return BadRequest(new { message = "Mã IMEI phải chứa đúng 15 chữ số từ 0-9." });
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
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

                _context.CustomerDevices.Add(device);
                await _context.SaveChangesAsync();

                orderItem.CustomerDeviceId = device.Id;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Kích hoạt mã IMEI thành công! Gói bảo hành đã có hiệu lực.",
                imei = cleanImei
            });
        }

        // LẤY GÓI BẢO HÀNH PHÙ HỢP CHO BIẾN THỂ SẢN PHẨM
        [HttpGet("variants/{variantId}")]
        public async Task<IActionResult> GetWarrantiesForVariant(int variantId)
        {
            var variant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == variantId);

            if (variant == null)
                return NotFound(new { message = "Không tìm thấy biến thể sản phẩm." });

            var ancestorCatIds = await GetAncestorCategoryIds(variant.Product.CategoryId);

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

        // ĐẶT MUA LẺ GÓI BẢO HÀNH
        [HttpPost("standalone/checkout")]
        [Authorize]
        public async Task<IActionResult> StandaloneCheckout([FromBody] StandaloneWarrantyCheckoutRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            var warranty = await _context.Warranties.FirstOrDefaultAsync(w => w.Id == request.WarrantyId && w.IsActive);
            if (warranty == null)
                return BadRequest(new { message = "Gói bảo hành không hợp lệ hoặc đã bị khóa." });

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện giao dịch." });
            }

            var cleanImei = string.IsNullOrWhiteSpace(request.Imei) ? "CHƯA_KÍCH_HOẠT" : request.Imei.Trim();

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
                    var variant = await _context.ProductVariants.Include(pv => pv.Product).FirstOrDefaultAsync(pv => pv.Id == request.VariantId);
                    if (variant != null)
                    {
                        customerDevice.ProductName = variant.Product.Name + " (" + variant.Name + ")";
                        customerDevice.VariantId = variant.Id;
                    }
                }

                _context.CustomerDevices.Add(customerDevice);
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
                _context.Orders.Add(newOrder);
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
                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

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
                return BadRequest(new { message = "Lỗi trong quá trình tạo đơn hàng: " + ex.Message });
            }
        }

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

