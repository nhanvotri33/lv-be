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
        public async Task<IActionResult> InspectOrderItem(int orderItemId, [FromBody] UpdateInspectionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
                return BadRequest(new { message = "Trạng thái thẩm định không hợp lệ." });

            var statusUpper = request.Status.ToUpper();
            if (statusUpper != "PASSED" && statusUpper != "FAILED")
                return BadRequest(new { message = "Trạng thái phải là 'PASSED' hoặc 'FAILED'." });

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.CustomerDevice)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

            if (orderItem == null)
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng." });

            if (!orderItem.WarrantyId.HasValue || !orderItem.CustomerDeviceId.HasValue)
                return BadRequest(new { message = "Sản phẩm này không đăng ký gói bảo hành cần thẩm định máy." });

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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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
                return BadRequest(new { message = "Lỗi trong quá trình cập nhật thẩm định: " + ex.Message });
            }
        }

        // ================= DANH SÁCH TẤT CẢ GÓI BẢO HÀNH (ADMIN) =================
        [HttpGet("packages")]
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await _context.Warranties
                .OrderByDescending(w => w.Id)
                .ToListAsync();
            return Ok(packages);
        }

        // ================= THÊM GÓI BẢO HÀNH MỚI (ADMIN) =================
        [HttpPost("packages")]
        public async Task<IActionResult> CreatePackage([FromBody] CreateWarrantyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.Name))
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            var exists = await _context.Warranties.AnyAsync(w => w.Code == request.Code);
            if (exists)
                return BadRequest(new { message = "Mã gói bảo hành này đã tồn tại." });

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

                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();

                // Tự động tạo rule toàn cầu cho gói này (áp dụng cho mọi dòng máy)
                var rule = new WarrantyPackageRule
                {
                    WarrantyId = warranty.Id,
                    ProductId = null,
                    CategoryId = null,
                    BrandId = null,
                    MinPrice = 0,
                    MaxPrice = null
                };
                _context.WarrantyPackageRules.Add(rule);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(warranty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi khi thêm gói bảo hành: " + ex.Message });
            }
        }

        // ================= CẬP NHẬT GÓI BẢO HÀNH (ADMIN) =================
        [HttpPut("packages/{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] UpdateWarrantyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
                return NotFound(new { message = "Không tìm thấy gói bảo hành." });

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

                await _context.SaveChangesAsync();
                return Ok(warranty);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật gói bảo hành: " + ex.Message });
            }
        }

        // ================= XÓA GÓI BẢO HÀNH (ADMIN) =================
        [HttpDelete("packages/{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
                return NotFound(new { message = "Không tìm thấy gói bảo hành." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rules = await _context.WarrantyPackageRules.Where(r => r.WarrantyId == id).ToListAsync();
                _context.WarrantyPackageRules.RemoveRange(rules);

                _context.Warranties.Remove(warranty);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Xóa gói bảo hành thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi khi xóa gói bảo hành: " + ex.Message });
            }
        }
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
    }
}
