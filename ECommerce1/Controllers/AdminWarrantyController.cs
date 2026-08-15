// ==========================================================================
// MODULE: AdminWarrantyController.cs
// MỤC ĐÍCH: API Controller phía Admin xử lý tiếp nhận, thẩm định thiết bị, cập nhật tình trạng kiểm tra (PASSED/REJECTED) và phê duyệt bảo hành.
// ==========================================================================
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Chì dành riêng cho Admin/Kỹ thuật viên cửa hàng
    public class AdminWarrantyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminWarrantyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= CẬP NHẬT TRẠNG THÁI THẨM ĐỊNH THIẾT BỊ (KTV) =================
        // Logic xử lý:
        // 1. Tìm dòng chi tiết đơn hàng (OrderItem) cần thẩm định cùng với Đơn hàng cha (Order).
        // 2. Kiểm tra xem dòng này có thực sự đính kèm gói bảo hành và thiết bị cũ cần kiểm tra hay không.
        // 3. Cập nhật InspectionStatus sang 'PASSED' (Đạt chuẩn) hoặc 'FAILED' (Không đạt chuẩn).
        // 4. Đặc biệt: Nếu KTV bấm Từ chối (FAILED), hệ thống tự động đổi trạng thái Đơn hàng cha sang 'Đã Hủy' (OrderStatusId = 5) để đóng hóa đơn và ngăn thanh toán.
        [HttpPut("order-items/{orderItemId}/inspect")]
        // [Hàm thực thi nghiệp vụ]: `InspectOrderItem` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> InspectOrderItem(int orderItemId, [FromBody] UpdateInspectionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Trạng thái thẩm định không hợp lệ." });

            var statusUpper = request.Status.ToUpper();
            if (statusUpper != "PASSED" && statusUpper != "FAILED")
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Trạng thái phải là 'PASSED' hoặc 'FAILED'." });

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.CustomerDevice)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

            if (orderItem == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng." });

            if (!orderItem.WarrantyId.HasValue || !orderItem.CustomerDeviceId.HasValue)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Sản phẩm này không đăng ký gói bảo hành cần thẩm định máy." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật trạng thái thẩm định
                orderItem.InspectionStatus = statusUpper;

                // Nếu admin cung cấp IMEI -> Cập nhật vào CustomerDevice
                if (!string.IsNullOrEmpty(request.Imei) && orderItem.CustomerDevice != null)
                {
                    orderItem.CustomerDevice.ImeiOrSerial = request.Imei;
                }

                // Nếu thẩm định thất bại -> Tự động hủy đơn hàng
                if (statusUpper == "FAILED")
                {
                    orderItem.Order.OrderStatusId = 5; // 5 = Cancelled (Đã hủy)
                    orderItem.Order.Note = "[HỦY TỰ ĐỘNG - THẨM ĐỊNH THẤT BẠI] " + (request.Note ?? "Thiết bị không đạt điều kiện ngoại quan.");
                }
                else
                {
                    // Nếu thẩm định đạt -> ghi chú lại vết thẩm định thành công
                    orderItem.Order.Note = "[THẨM ĐỊNH THÀNH CÔNG] " + (request.Note ?? "Thiết bị ngoại quan đạt chuẩn bảo hành.");
                }

                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new
                {
                    message = "Cập nhật trạng thái thẩm định thành công.",
                    orderId = orderItem.OrderId,
                    inspectionStatus = orderItem.InspectionStatus,
                    orderStatusId = orderItem.Order.OrderStatusId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Lỗi trong quá trình cập nhật thẩm định: " + ex.Message });
            }
        }

        // ================= DANH SÁCH TẤT CẢ GÓI BẢO HÀNH (ADMIN) =================
        [HttpGet("packages")]
        // [Hàm thực thi nghiệp vụ]: `GetAllPackages` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await _context.Warranties
                .OrderByDescending(w => w.Id)
                .Select(w => new
                {
                    w.Id,
                    w.Code,
                    w.Name,
                    w.Description,
                    w.TermsHtml,
                    w.DurationMonths,
                    w.BasePrice,
                    w.RequiresInspection,
                    w.IsActive,
                    w.CreatedAt,
                    w.UpdatedAt,
                    Rules = _context.WarrantyPackageRules
                        .Where(r => r.WarrantyId == w.Id)
                        .Select(r => new
                        {
                            r.BrandId,
                            r.CategoryId,
                            r.ProductId,
                            r.MinPrice,
                            r.MaxPrice
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(packages);
        }

        // ================= THÊM GÓI BẢO HÀNH MỚI (ADMIN) =================
        [HttpPost("packages")]
        // [Hàm thực thi nghiệp vụ]: `CreatePackage` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CreatePackage([FromBody] CreateWarrantyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.Name))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var exists = await _context.Warranties.AnyAsync(w => w.Code == request.Code);
            if (exists)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Mã gói bảo hành này đã tồn tại." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var warranty = new Warranty
                {
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    TermsHtml = request.TermsHtml,
                    DurationMonths = request.DurationMonths,
                    BasePrice = request.BasePrice,
                    RequiresInspection = request.RequiresInspection,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Warranties.Add(warranty);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // Tạo rule từ thông tin gửi lên
                // LOGIC CẤU HÌNH QUY TẮC RÀNG BUỘC KHI LƯU:
                // - ProductId = null, CategoryId = null, BrandId = null -> Không có bất kỳ ràng buộc nào về máy/danh mục/hãng (áp dụng toàn cầu).
                // - Nếu chọn cụ thể (khác null) -> Chỉ kích hoạt khi thông tin biến thể máy khớp với ID được chọn.
                // - MinPrice mặc định là 0 (nếu bỏ trống), MaxPrice = null -> Không giới hạn khoảng giá máy tối đa.
                var rule = new WarrantyPackageRule
                {
                    WarrantyId = warranty.Id,
                    ProductId = request.Rules?.ProductId,
                    CategoryId = request.Rules?.CategoryId,
                    BrandId = request.Rules?.BrandId,
                    MinPrice = request.Rules?.MinPrice ?? 0,
                    MaxPrice = request.Rules?.MaxPrice
                };
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.WarrantyPackageRules.Add(rule);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(warranty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Lỗi khi thêm gói bảo hành: " + ex.Message });
            }
        }

        // ================= CẬP NHẬT GÓI BẢO HÀNH (ADMIN) =================
        [HttpPut("packages/{id}")]
        // [Hàm thực thi nghiệp vụ]: `UpdatePackage` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] UpdateWarrantyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy gói bảo hành." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                warranty.Name = request.Name;
                warranty.Description = request.Description;
                warranty.TermsHtml = request.TermsHtml;
                warranty.DurationMonths = request.DurationMonths;
                warranty.BasePrice = request.BasePrice;
                warranty.RequiresInspection = request.RequiresInspection;
                warranty.IsActive = request.IsActive;
                warranty.UpdatedAt = DateTime.UtcNow;

                // Cập nhật hoặc tạo rule liên kết
                // LOGIC CẬP NHẬT QUY TẮC RÀNG BUỘC KHI SỬA GÓI:
                // - Nếu admin bỏ chọn (chọn "Tất cả...") -> ID tương ứng lưu NULL vào database.
                // - MinPrice & MaxPrice được cập nhật theo số lượng VNĐ thực tế từ FE gửi lên.
                var rule = await _context.WarrantyPackageRules.FirstOrDefaultAsync(r => r.WarrantyId == id);
                if (rule == null)
                {
                    rule = new WarrantyPackageRule { WarrantyId = id };
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    _context.WarrantyPackageRules.Add(rule);
                }
                rule.ProductId = request.Rules?.ProductId;
                rule.BrandId = request.Rules?.BrandId;
                rule.CategoryId = request.Rules?.CategoryId;
                rule.MinPrice = request.Rules?.MinPrice ?? 0;
                rule.MaxPrice = request.Rules?.MaxPrice;

                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(warranty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Lỗi khi cập nhật gói bảo hành: " + ex.Message });
            }
        }

        // ================= XÓA GÓI BẢO HÀNH (ADMIN) =================
        [HttpDelete("packages/{id}")]
        // [Hàm thực thi nghiệp vụ]: `DeletePackage` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> DeletePackage(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy gói bảo hành." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                var rules = await _context.WarrantyPackageRules.Where(r => r.WarrantyId == id).ToListAsync();
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.WarrantyPackageRules.RemoveRange(rules);

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Warranties.Remove(warranty);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { message = "Xóa gói bảo hành thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Lỗi khi xóa gói bảo hành: " + ex.Message });
            }
        }

        // ================= DANH SÁCH BẢO HÀNH & THIẾT BỊ KHÁCH HÀNG (DÙNG CHO ADMIN/KTV TIẾP CẬN SỬA CHỮA) =================
        [HttpGet("customer-warranties")]
        // [Hàm thực thi nghiệp vụ]: `GetCustomerWarranties` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetCustomerWarranties(
            [FromQuery] string? search = null,
            [FromQuery] string? imei = null,
            [FromQuery] string? status = null)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.User)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.OrderStatus)
                .Include(oi => oi.Warranty)
                .Include(oi => oi.CustomerDevice)
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(oi => oi.WarrantyId.HasValue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(oi =>
                    (oi.Order.User != null && (oi.Order.User.Username.ToLower().Contains(s) || oi.Order.User.Email.ToLower().Contains(s))) ||
                    oi.Order.ReceiverName.ToLower().Contains(s) ||
                    oi.Order.ReceiverPhone.Contains(s) ||
                    (oi.CustomerDevice != null && oi.CustomerDevice.ProductName.ToLower().Contains(s))
                );
            }

            if (!string.IsNullOrWhiteSpace(imei))
            {
                var im = imei.Trim();
                query = query.Where(oi => oi.CustomerDevice != null && oi.CustomerDevice.ImeiOrSerial.Contains(im));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim().ToUpper();
                if (st == "ACTIVATED")
                {
                    query = query.Where(oi => oi.CustomerDevice != null && !string.IsNullOrEmpty(oi.CustomerDevice.ImeiOrSerial) && oi.CustomerDevice.ImeiOrSerial != "CHƯA_KÍCH_HOẠT");
                }
                else if (st == "PENDING")
                {
                    query = query.Where(oi => oi.CustomerDevice == null || string.IsNullOrEmpty(oi.CustomerDevice.ImeiOrSerial) || oi.CustomerDevice.ImeiOrSerial == "CHƯA_KÍCH_HOẠT");
                }
            }

            var result = await query
                .OrderByDescending(oi => oi.Order.CreatedAt)
                .Select(oi => new
                {
                    OrderItemId = oi.Id,
                    OrderId = oi.OrderId,
                    OrderDate = oi.Order.CreatedAt,
                    OrderStatusId = oi.Order.OrderStatusId,
                    OrderStatusName = oi.Order.OrderStatus != null ? oi.Order.OrderStatus.Name : "N/A",
                    
                    // Thông tin khách hàng
                    UserId = oi.Order.UserId,
                    UserName = oi.Order.User != null ? oi.Order.User.Username : oi.Order.ReceiverName,
                    UserEmail = oi.Order.User != null ? oi.Order.User.Email : "",
                    ReceiverName = oi.Order.ReceiverName,
                    ReceiverPhone = oi.Order.ReceiverPhone,

                    // Thông tin máy & IMEI
                    CustomerDeviceId = oi.CustomerDeviceId,
                    ProductName = oi.CustomerDevice != null ? oi.CustomerDevice.ProductName : (oi.ProductVariant != null ? oi.ProductVariant.Product.Name + " (" + oi.ProductVariant.Name + ")" : "Thiết bị"),
                    Imei = oi.CustomerDevice != null ? oi.CustomerDevice.ImeiOrSerial : "CHƯA_KÍCH_HOẠT",
                    IsActivated = oi.CustomerDevice != null && !string.IsNullOrEmpty(oi.CustomerDevice.ImeiOrSerial) && oi.CustomerDevice.ImeiOrSerial != "CHƯA_KÍCH_HOẠT",

                    // Thông tin bảo hành
                    WarrantyId = oi.WarrantyId,
                    WarrantyName = oi.Warranty != null ? oi.Warranty.Name : "Bảo hành mở rộng",
                    WarrantyCode = oi.Warranty != null ? oi.Warranty.Code : "",
                    DurationMonths = oi.Warranty != null ? oi.Warranty.DurationMonths : 12,
                    WarrantyPrice = oi.WarrantyPrice,
                    InspectionStatus = oi.InspectionStatus,
                    ExpireDate = oi.Order.CreatedAt.AddMonths(oi.Warranty != null ? oi.Warranty.DurationMonths : 12),
                    IsExpired = DateTime.UtcNow > oi.Order.CreatedAt.AddMonths(oi.Warranty != null ? oi.Warranty.DurationMonths : 12),

                    // Ghi chú xử lý / Tiếp cận sửa chữa
                    OrderNote = oi.Order.Note
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(result);
        }

        // ================= CẬP NHẬT MÃ IMEI VÀ GHI CHÚ TIẾP CẬN SỬA CHỮA (ADMIN) =================
        [HttpPut("update-device-imei")]
        // [Hàm thực thi nghiệp vụ]: `UpdateDeviceImei` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdateDeviceImei([FromBody] AdminUpdateImeiRequest request)
        {
            if (request == null || request.OrderItemId <= 0)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Thông tin không hợp lệ." });
            }

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.CustomerDevice)
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(oi => oi.Id == request.OrderItemId);

            if (orderItem == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy thông tin gói bảo hành này." });
            }

            if (!string.IsNullOrWhiteSpace(request.Imei))
            {
                var cleanImei = request.Imei.Trim();
                if (orderItem.CustomerDevice != null)
                {
                    orderItem.CustomerDevice.ImeiOrSerial = cleanImei;
                }
                else
                {
                    var prodName = orderItem.ProductVariant != null
                        ? orderItem.ProductVariant.Product.Name + " (" + orderItem.ProductVariant.Name + ")"
                        : "Thiết bị bảo hành";

                    var device = new CustomerDevice
                    {
                        UserId = orderItem.Order.UserId,
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
            }

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                orderItem.Order.Note = request.Note;
            }

            if (!string.IsNullOrWhiteSpace(request.InspectionStatus))
            {
                orderItem.InspectionStatus = request.InspectionStatus;
            }

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Cập nhật thông tin IMEI & Ghi chú tiếp cận sửa chữa thành công!" });
        }
    }

    public class AdminUpdateImeiRequest
    {
        public int OrderItemId { get; set; }
        public string? Imei { get; set; }
        public string? Note { get; set; }
        public string? InspectionStatus { get; set; }
    }


    public class UpdateInspectionRequest
    {
        public string Status { get; set; } // "PASSED" or "FAILED"
        public string? Note { get; set; }
        public string? Imei { get; set; }
    }

    public class CreateWarrantyRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TermsHtml { get; set; }
        public int DurationMonths { get; set; }
        public decimal BasePrice { get; set; }
        public bool RequiresInspection { get; set; }
        public bool IsActive { get; set; } = true;
        public WarrantyRulePayload? Rules { get; set; }
    }

    public class UpdateWarrantyRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TermsHtml { get; set; }
        public int DurationMonths { get; set; }
        public decimal BasePrice { get; set; }
        public bool RequiresInspection { get; set; }
        public bool IsActive { get; set; }
        public WarrantyRulePayload? Rules { get; set; }
    }

    public class WarrantyRulePayload
    {
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public decimal MinPrice { get; set; } = 0;
        public decimal? MaxPrice { get; set; }
    }
}
