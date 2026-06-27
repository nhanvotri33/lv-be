using ECommerce.Models;
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
    [Authorize]
    public class PromotionUsageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionUsageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem lịch sử sử dụng mã giảm giá của bản thân
        [HttpGet("my-usages")]
        public async Task<IActionResult> GetMyUsages()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var usages = await _context.PromotionUsages
                .Include(pu => pu.Promotion)
                .Where(pu => pu.UserId == userId)
                .Select(pu => new 
                {
                    Id = pu.Id,
                    UsedAt = pu.UsedAt,
                    PromotionCode = pu.Promotion != null ? pu.Promotion.Code : "Đã xóa",
                    DiscountValue = pu.Promotion != null ? pu.Promotion.DiscountValue : 0
                })
                .ToListAsync();

            return Ok(usages);
        }
    }
}
