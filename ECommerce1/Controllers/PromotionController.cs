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
        public async Task<IActionResult> GetAll()
        {
            bool isAdmin = User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin");
            
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
                UsedCount = isAdmin ? p.UsedCount : 0
            }).ToList();

            return Ok(response);
        }

        // Tạo mã giảm giá (Chỉ Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] PromotionRequest request)
        {
            if (await _context.Promotions.AnyAsync(p => p.Code == request.Code))
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
                UsedCount = 0
            };

            _context.Promotions.Add(newPromo);
            await _context.SaveChangesAsync();

            return Ok("Tạo mã khuyến mãi thành công.");
        }

        // Cập nhật trạng thái / thông tin mã giảm giá (Chỉ Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PromotionRequest request)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            promo.Code = request.Code.ToUpper();
            promo.DiscountType = request.DiscountType.ToUpper();
            promo.DiscountValue = request.DiscountValue;
            promo.StartDate = request.StartDate;
            promo.EndDate = request.EndDate;
            promo.IsActive = request.IsActive;
            promo.UsageLimit = request.UsageLimit;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật mã khuyến mãi thành công.");
        }

        // Xóa mã giảm giá (Chỉ Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound("Không tìm thấy mã khuyến mãi.");

            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            return Ok("Xóa mã khuyến mãi thành công.");
        }

        // Kiểm tra và áp dụng mã giảm giá
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidatePromotionRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                return BadRequest("Mã giảm giá trống.");

            var code = request.Code.ToUpper().Trim();
            var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
            
            if (promo == null)
                return NotFound("Mã giảm giá không tồn tại.");

            if (!promo.IsActive)
                return BadRequest("Mã giảm giá đã bị vô hiệu hóa.");

            var now = DateTime.UtcNow;
            if (now < promo.StartDate || now > promo.EndDate)
                return BadRequest("Mã giảm giá đã hết hạn hoặc chưa tới thời gian áp dụng.");

            if (promo.UsageLimit > 0 && promo.UsedCount >= promo.UsageLimit)
                return BadRequest("Mã giảm giá đã hết lượt sử dụng.");

            // Kiểm tra xem User đã dùng mã này bao giờ chưa
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                bool hasUsed = await _context.PromotionUsages.AnyAsync(pu => pu.PromotionId == promo.Id && pu.UserId == userId);
                if (hasUsed)
                    return BadRequest("Bạn đã sử dụng mã giảm giá này rồi.");
            }

            decimal discountValue = 0;
            if (promo.DiscountType.ToUpper() == "PERCENTAGE")
            {
                discountValue = request.SubTotal * (promo.DiscountValue / 100);
            }
            else if (promo.DiscountType.ToUpper() == "FIXED_AMOUNT")
            {
                discountValue = promo.DiscountValue;
            }

            if (discountValue > request.SubTotal)
                discountValue = request.SubTotal;

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
