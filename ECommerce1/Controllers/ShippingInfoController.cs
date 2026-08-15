// ==========================================================================
// MODULE: ShippingInfoController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module ShippingInfoController
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.ShippingInfo;
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
    [Authorize] // Bảo vệ toàn bộ thông tin địa chỉ giao hàng
    public class ShippingInfoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShippingInfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= LẤY DANH SÁCH ĐỊA CHỈ GIAO HÀNG =================
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetMyShippingInfos` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetMyShippingInfos()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            var infos = await _context.ShippingInfos
                .Include(s => s.Ward).ThenInclude(w => w.Province)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.IsDefault) // Địa chỉ mặc định xếp lên đầu
                .ThenByDescending(s => s.CreatedAt)
                .Select(s => new ShippingInfoResponse
                {
                    Id = s.Id,
                    RecipientName = s.RecipientName,
                    PhoneNumber = s.PhoneNumber,
                    AddressLine = s.AddressLine,
                    WardId = s.WardId,
                    WardName = s.Ward != null ? s.Ward.Name : "",
                    ProvinceId = s.Ward != null ? s.Ward.ProvinceId : "",
                    ProvinceName = s.Ward != null && s.Ward.Province != null ? s.Ward.Province.Name : "",
                    IsDefault = s.IsDefault,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(infos);
        }

        // ================= TẠO ĐỊA CHỈ GIAO HÀNG MỚI =================
        [HttpPost]
        // [Hàm thực thi nghiệp vụ]: `Create` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Create([FromBody] ShippingInfoRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            // Nếu muốn set địa chỉ này làm mặc định, phải tắt mặc định của các địa chỉ cũ
            if (request.IsDefault)
            {
                var oldDefaults = await _context.ShippingInfos
                    .Where(s => s.UserId == userId && s.IsDefault)
                    .ToListAsync();
                
                foreach (var old in oldDefaults)
                {
                    old.IsDefault = false;
                }
            }
            else
            {
                // Nếu chưa có địa chỉ nào, bắt buộc địa chỉ đầu tiên phải là mặc định
                bool hasAny = await _context.ShippingInfos.AnyAsync(s => s.UserId == userId);
                if (!hasAny) request.IsDefault = true;
            }

            await ECommerce1.Services.VietnamLocationService.EnsureLocationExistsAsync(_context, request.WardId);

            var newInfo = new ShippingInfo
            {
                UserId = userId,
                RecipientName = request.RecipientName,
                PhoneNumber = request.PhoneNumber,
                AddressLine = request.AddressLine,
                WardId = request.WardId,
                IsDefault = request.IsDefault,
                Latitude = request.Latitude,   // Lưu tọa độ từ Goong Maps
                Longitude = request.Longitude,  // Lưu tọa độ từ Goong Maps
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.ShippingInfos.Add(newInfo);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Tạo địa chỉ giao hàng thành công.");
        }

        // ================= CẬP NHẬT ĐỊA CHỈ GIAO HÀNG =================
        [HttpPut("{id}")]
        // [Hàm thực thi nghiệp vụ]: `Update` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Update(int id, [FromBody] ShippingInfoRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            var info = await _context.ShippingInfos
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (info == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy địa chỉ này.");

            if (info.UserId != userId)
                // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                return StatusCode(403, "Bạn không có quyền sửa địa chỉ của người khác.");


            // Nếu cập nhật thành mặc định, bỏ mặc định ở các địa chỉ khác
            if (request.IsDefault && !info.IsDefault)
            {
                var oldDefaults = await _context.ShippingInfos
                    .Where(s => s.UserId == userId && s.IsDefault)
                    .ToListAsync();
                
                foreach (var old in oldDefaults)
                {
                    old.IsDefault = false;
                }
            }

            await ECommerce1.Services.VietnamLocationService.EnsureLocationExistsAsync(_context, request.WardId);

            info.RecipientName = request.RecipientName;
            info.PhoneNumber = request.PhoneNumber;
            info.AddressLine = request.AddressLine;
            info.WardId = request.WardId;
            info.IsDefault = request.IsDefault;
            // Chỉ cập nhật tọa độ nếu có dữ liệu mới từ Goong Maps
            if (request.Latitude.HasValue) info.Latitude = request.Latitude;
            if (request.Longitude.HasValue) info.Longitude = request.Longitude;
            info.UpdatedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cập nhật địa chỉ thành công.");
        }

        // ================= XÓA ĐỊA CHỈ GIAO HÀNG =================
        [HttpDelete("{id}")]
        // [Hàm thực thi nghiệp vụ]: `Delete` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            var info = await _context.ShippingInfos
                .FirstOrDefaultAsync(s => s.Id == id);

            if (info == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy địa chỉ này.");

            if (info.UserId != userId)
                // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                return StatusCode(403, "Bạn không có quyền xóa địa chỉ của người khác.");

            bool wasDefault = info.IsDefault;

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.ShippingInfos.Remove(info);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // Nếu địa chỉ bị xóa là địa chỉ mặc định, tự động gán địa chỉ còn lại gần nhất làm mặc định
            if (wasDefault)
            {
                var remainingDefault = await _context.ShippingInfos
                    .Where(s => s.UserId == userId && s.IsDefault)
                    .FirstOrDefaultAsync();

                if (remainingDefault == null)
                {
                    var newDefault = await _context.ShippingInfos
                        .Where(s => s.UserId == userId)
                        .OrderByDescending(s => s.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (newDefault != null)
                    {
                        newDefault.IsDefault = true;
                        // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Đã xóa địa chỉ thành công.");
        }
    }
}
