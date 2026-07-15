using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductComboController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductComboController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductCombos(int productId)
        {
            var currentDate = DateTime.UtcNow;

            var combos = await _context.ProductCombos
                .Include(pc => pc.ComboItems)
                    .ThenInclude(ci => ci.Product)
                .Where(pc => pc.IsActive && pc.StartDate <= currentDate && pc.EndDate >= currentDate)
                .Where(pc => pc.ComboItems.Any(ci => ci.ProductId == productId && ci.IsMain))
                .Select(pc => new {
                    pc.Id,
                    pc.Name,
                    pc.Description,
                    Items = pc.ComboItems.Select(ci => new {
                        ci.ProductId,
                        ProductName = ci.Product.Name,
                        BasePrice = ci.Product.BasePrice,
                        ThumbnailImage = ci.Product.ThumbnailImage,
                        ci.IsMain,
                        ci.DiscountType,
                        ci.DiscountValue,
                        ComboPrice = ci.DiscountType == "Percentage" 
                            ? ci.Product.BasePrice * (1 - ci.DiscountValue / 100)
                            : Math.Max(0, ci.Product.BasePrice - ci.DiscountValue)
                    })
                })
                .ToListAsync();

            return Ok(combos);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCombos()
        {
            var currentDate = DateTime.UtcNow;

            var combos = await _context.ProductCombos
                .Include(pc => pc.ComboItems)
                .Where(pc => pc.IsActive && pc.StartDate <= currentDate && pc.EndDate >= currentDate)
                .Select(pc => new {
                    pc.Id,
                    pc.Name,
                    pc.Description,
                    Items = pc.ComboItems.Select(ci => new {
                        ci.ProductId,
                        ci.IsMain,
                        ci.DiscountType,
                        ci.DiscountValue
                    })
                })
                .ToListAsync();

            return Ok(combos);
        }

        // --- ADMIN ENDPOINTS ---

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCombos()
        {
            var combos = await _context.ProductCombos
                .Include(c => c.ComboItems)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.StartDate,
                    c.EndDate,
                    c.IsActive,
                    c.CreatedAt,
                    ComboItems = c.ComboItems.Select(ci => new {
                        ci.Id,
                        ci.ProductId,
                        ci.IsMain,
                        ci.DiscountType,
                        ci.DiscountValue
                    })
                })
                .ToListAsync();
            return Ok(combos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetComboById(int id)
        {
            var combo = await _context.ProductCombos
                .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null) return NotFound("Combo không tồn tại.");

            var response = new
            {
                combo.Id,
                combo.Name,
                combo.Description,
                combo.StartDate,
                combo.EndDate,
                combo.IsActive,
                Items = combo.ComboItems.Select(ci => new
                {
                    ci.Id,
                    ci.ProductId,
                    ProductName = ci.Product?.Name,
                    ci.IsMain,
                    ci.DiscountType,
                    ci.DiscountValue
                })
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCombo([FromBody] ECommerce1.DTOs.Combo.ProductComboRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Kiểm tra phải có đúng 1 sản phẩm chính
            if (request.Items.Count(i => i.IsMain) != 1)
            {
                return BadRequest("Combo phải có chính xác 1 sản phẩm chính.");
            }

            var combo = new ProductCombo
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ComboItems = request.Items.Select(i => new ProductComboItem
                {
                    ProductId = i.ProductId,
                    IsMain = i.IsMain,
                    DiscountType = i.DiscountType,
                    DiscountValue = i.DiscountValue
                }).ToList()
            };

            _context.ProductCombos.Add(combo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm Combo thành công.", id = combo.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCombo(int id, [FromBody] ECommerce1.DTOs.Combo.ProductComboRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var combo = await _context.ProductCombos
                .Include(c => c.ComboItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null) return NotFound("Combo không tồn tại.");

            if (request.Items.Count(i => i.IsMain) != 1)
            {
                return BadRequest("Combo phải có chính xác 1 sản phẩm chính.");
            }

            combo.Name = request.Name;
            combo.Description = request.Description;
            combo.StartDate = request.StartDate;
            combo.EndDate = request.EndDate;
            combo.IsActive = request.IsActive;
            combo.UpdatedAt = DateTime.UtcNow;

            // Xóa hết item cũ và thêm item mới
            _context.ProductComboItems.RemoveRange(combo.ComboItems);
            
            foreach (var item in request.Items)
            {
                combo.ComboItems.Add(new ProductComboItem
                {
                    ProductId = item.ProductId,
                    IsMain = item.IsMain,
                    DiscountType = item.DiscountType,
                    DiscountValue = item.DiscountValue
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật Combo thành công.", id = combo.Id });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCombo(int id)
        {
            var combo = await _context.ProductCombos.FindAsync(id);
            if (combo == null) return NotFound();

            _context.ProductCombos.Remove(combo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa Combo thành công." });
        }
    }
}
