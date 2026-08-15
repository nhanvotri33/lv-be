// ==========================================================================
// MODULE: CategoryBrandDefaultController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module CategoryBrandDefaultController
// ==========================================================================
      using ECommerce.Models;
using ECommerce1.DTOs.CategoryBrandDefault;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryBrandDefaultController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryBrandDefaultController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CategoryBrandDefault/category/5
        [HttpGet("category/{categoryId}")]
        // [Hàm thực thi nghiệp vụ]: `GetByCategory` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var defaults = await _context.CategoryBrandDefaults
                .Include(cbd => cbd.Category)
                .Include(cbd => cbd.Brand)
                .Where(cbd => cbd.CategoryId == categoryId)
                .Select(cbd => new CategoryBrandDefaultResponse
                {
                    Id = cbd.Id,
                    CategoryId = cbd.CategoryId,
                    CategoryName = cbd.Category != null ? cbd.Category.Name : string.Empty,
                    BrandId = cbd.BrandId,
                    BrandName = cbd.Brand != null ? cbd.Brand.Name : string.Empty,
                    DefaultSpecs = cbd.DefaultSpecs,
                    CreatedAt = cbd.CreatedAt,
                    UpdatedAt = cbd.UpdatedAt
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(defaults);
        }

        // GET: api/CategoryBrandDefault/category/5/brand/3
        [HttpGet("category/{categoryId}/brand/{brandId}")]
        // [Hàm thực thi nghiệp vụ]: `GetByCategoryAndBrand` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetByCategoryAndBrand(int categoryId, int brandId)
        {
            var match = await _context.CategoryBrandDefaults
                .Include(cbd => cbd.Category)
                .Include(cbd => cbd.Brand)
                .FirstOrDefaultAsync(cbd => cbd.CategoryId == categoryId && cbd.BrandId == brandId);

            if (match == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy cấu hình thông số mặc định cho cặp Danh mục và Thương hiệu này.");
            }

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new CategoryBrandDefaultResponse
            {
                Id = match.Id,
                CategoryId = match.CategoryId,
                CategoryName = match.Category != null ? match.Category.Name : string.Empty,
                BrandId = match.BrandId,
                BrandName = match.Brand != null ? match.Brand.Name : string.Empty,
                DefaultSpecs = match.DefaultSpecs,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt
            });
        }

        // POST: api/CategoryBrandDefault
        [HttpPost]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Upsert` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Upsert([FromBody] CategoryBrandDefaultRequest request)
        {
            if (!ModelState.IsValid)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(ModelState);
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Danh mục không tồn tại.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == request.BrandId);
            if (!brandExists)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Thương hiệu không tồn tại.");
            }

            var existing = await _context.CategoryBrandDefaults
                .FirstOrDefaultAsync(cbd => cbd.CategoryId == request.CategoryId && cbd.BrandId == request.BrandId);

            if (existing != null)
            {
                existing.DefaultSpecs = request.DefaultSpecs;
                existing.UpdatedAt = DateTime.UtcNow;
                
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
                
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { message = "Cập nhật cấu hình mặc định thành công.", id = existing.Id });
            }
            else
            {
                var newDefault = new CategoryBrandDefault
                {
                    CategoryId = request.CategoryId,
                    BrandId = request.BrandId,
                    DefaultSpecs = request.DefaultSpecs,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.CategoryBrandDefaults.Add(newDefault);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { message = "Thêm cấu hình mặc định thành công.", id = newDefault.Id });
            }
        }

        // DELETE: api/CategoryBrandDefault/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Delete` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Delete(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var match = await _context.CategoryBrandDefaults.FindAsync(id);
            if (match == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy cấu hình cần xóa.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.CategoryBrandDefaults.Remove(match);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Xóa cấu hình mặc định thành công." });
        }
    }
}
