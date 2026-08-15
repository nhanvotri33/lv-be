// ==========================================================================
// MODULE: PromotionController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module PromotionController
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.Promotion;
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
    public class PromotionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách các mã giảm giá
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetAll` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAll()
        {
            bool isAdmin = User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin");
            
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var query = _context.Promotions.AsQueryable();

            // Nếu là User thường (hoặc khách chưa đăng nhập): Chỉ hiển thị mã CÒN HIỆU LỰC
            if (!isAdmin)
            {
                var now = DateTime.UtcNow;
                query = query.Where(p => p.IsActive 
                                      && p.StartDate <= now 
                                      && p.EndDate >= now
                                      && (p.UsageLimit == 0 || p.UsedCount < p.UsageLimit));
            }

            // Lấy dữ liệu từ DB
            var promos = await query.ToListAsync();

            // Ánh xạ sang DTO
            var response = promos.Select(p => new PromotionResponse
            {
                Id = p.Id,
                Code = p.Code,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                // Bảo mật dữ liệu: User thường không cần biết chính xác mình có bao nhiêu mã và đã xài bao nhiêu
                UsageLimit = isAdmin ? p.UsageLimit : 0,
                UsedCount = isAdmin ? p.UsedCount : 0,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                MaxPerUser = p.MaxPerUser
            }).ToList();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }

        // Tạo mã giảm giá (Chỉ Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Create` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Create([FromBody] PromotionRequest request)
        {
            // =========================================================================
            // [XỬ LÝ MÃ KHUYẾN MÃI - BACK-END]
            // - Kiểm tra xem mã khuyến mãi (Voucher Code) do Admin nhập vào đã tồn tại chưa.
            // - Không cho phép trùng mã khuyến mãi để đảm bảo tính duy nhất khi áp dụng voucher.
            // =========================================================================
            if (await _context.Promotions.AnyAsync(p => p.Code == request.Code))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã khuyến mãi này đã tồn tại.");

            var newPromo = new Promotion
            {
                Code = request.Code.ToUpper(),
                DiscountType = request.DiscountType.ToUpper(),
                DiscountValue = request.DiscountValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                UsageLimit = request.UsageLimit,
                UsedCount = 0,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                MaxPerUser = request.MaxPerUser
            };

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Promotions.Add(newPromo);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Tạo mã khuyến mãi thành công.");
        }

        // Cập nhật trạng thái / thông tin mã giảm giá (Chỉ Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Update` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Update(int id, [FromBody] PromotionRequest request)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var promo = await _context.Promotions.FindAsync(id);
            // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
            if (promo == null) return NotFound();

            promo.Code = request.Code.ToUpper();
            promo.DiscountType = request.DiscountType.ToUpper();
            promo.DiscountValue = request.DiscountValue;
            promo.StartDate = request.StartDate;
            promo.EndDate = request.EndDate;
            promo.IsActive = request.IsActive;
            promo.UsageLimit = request.UsageLimit;
            promo.MinOrderAmount = request.MinOrderAmount;
            promo.MaxDiscountAmount = request.MaxDiscountAmount;
            promo.MaxPerUser = request.MaxPerUser;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cập nhật mã khuyến mãi thành công.");
        }

        // Xóa mã giảm giá (Chỉ Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Delete` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Delete(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var promo = await _context.Promotions.FindAsync(id);
            // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
            if (promo == null) return NotFound("Không tìm thấy mã khuyến mãi.");

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Promotions.Remove(promo);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Xóa mã khuyến mãi thành công.");
        }

        // Kiểm tra và áp dụng mã giảm giá
        [HttpPost("validate")]
        // [Hàm thực thi nghiệp vụ]: `Validate` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Validate([FromBody] ValidatePromotionRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã giảm giá trống.");

            var code = request.Code.ToUpper().Trim();
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
            
            if (promo == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Mã giảm giá không tồn tại.");

            if (!promo.IsActive)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã giảm giá đã bị vô hiệu hóa.");

            var now = DateTime.UtcNow;
            if (now < promo.StartDate || now > promo.EndDate)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã giảm giá đã hết hạn hoặc chưa tới thời gian áp dụng.");

            if (promo.UsageLimit > 0 && promo.UsedCount >= promo.UsageLimit)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã giảm giá đã hết lượt sử dụng.");

            if (promo.MinOrderAmount.HasValue && request.SubTotal < promo.MinOrderAmount.Value)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest($"Đơn hàng chưa đạt giá trị tối thiểu {promo.MinOrderAmount.Value:N0}đ để áp dụng mã này.");

            // Kiểm tra xem User đã dùng mã này bao nhiêu lần
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                int maxAllowed = promo.MaxPerUser.HasValue && promo.MaxPerUser.Value > 0 ? promo.MaxPerUser.Value : 1;
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                int userUsageCount = await _context.PromotionUsages.CountAsync(pu => pu.PromotionId == promo.Id && pu.UserId == userId);
                if (userUsageCount >= maxAllowed)
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest($"Bạn đã sử dụng mã giảm giá này tối đa {maxAllowed} lần cho phép.");
            }

            decimal discountValue = 0;
            if (promo.DiscountType.ToUpper() == "PERCENTAGE")
            {
                discountValue = request.SubTotal * (promo.DiscountValue / 100);
                if (promo.MaxDiscountAmount.HasValue && discountValue > promo.MaxDiscountAmount.Value)
                {
                    discountValue = promo.MaxDiscountAmount.Value;
                }
            }
            else if (promo.DiscountType.ToUpper() == "FIXED_AMOUNT")
            {
                discountValue = promo.DiscountValue;
            }

            if (discountValue > request.SubTotal)
                discountValue = request.SubTotal;

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new
            {
                promo.Id,
                promo.Code,
                promo.DiscountType,
                DiscountValue = promo.DiscountValue,
                DiscountAmount = discountValue
            });
        }
    }

    public class ValidatePromotionRequest
    {
        public string Code { get; set; }
        public decimal SubTotal { get; set; }
    }
}
