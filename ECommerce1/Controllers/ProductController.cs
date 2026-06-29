using ECommerce.Models;
using ECommerce1.DTOs.Product;
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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<HashSet<int>> GetValidCategoryIdsAsync()
        {
            var allCats = await _context.Categories.ToListAsync();
            var validIds = new HashSet<int>();
            
            var level1 = allCats.Where(c => c.ParentId == null && c.IsActive != false).ToList();
            foreach (var c1 in level1)
            {
                validIds.Add(c1.Id);
                
                var level2 = allCats.Where(c => c.ParentId == c1.Id && c.IsActive != false).ToList();
                foreach (var c2 in level2)
                {
                    validIds.Add(c2.Id);
                    
                    var level3 = allCats.Where(c => c.ParentId == c2.Id && c.IsActive != false).ToList();
                    foreach (var c3 in level3)
                    {
                        validIds.Add(c3.Id);
                    }
                }
            }
            
            return validIds;
        }

        // ================= READ: Ai cũng xem được =================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var validCategoryIds = await GetValidCategoryIdsAsync();

            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive != false && 
                                         validCategoryIds.Contains(p.CategoryId) &&
                                         (p.BrandId == null || p.Brand.IsActive != false));
            }

            var productsList = await query.ToListAsync();

            var products = productsList.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ProductCode = p.ProductCode,
                Description = p.Description,
                BasePrice = p.BasePrice,
                OriginalPrice = p.OriginalPrice,
                TotalStock = p.TotalStock,
                ReservedStock = p.ReservedStock,
                AvailableStock = p.AvailableStock,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                ThumbnailImage = p.ThumbnailImage,
                MainImage = p.MainImage,
                Images = p.Images,
                IsAvailable = p.IsActive && validCategoryIds.Contains(p.CategoryId) && (p.BrandId == null || (p.Brand != null && p.Brand.IsActive != false)),
                BrandIsActive = p.Brand != null ? (bool?)p.Brand.IsActive : null,
                AverageRating = p.Reviews != null && p.Reviews.Any(r => !r.IsHidden) ? p.Reviews.Where(r => !r.IsHidden).Average(r => r.Rating) : 5.0,
                ReviewCount = p.Reviews != null ? p.Reviews.Count(r => !r.IsHidden) : 0
            })
            .ToList();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Không tìm thấy sản phẩm.");

            var validCategoryIds = await GetValidCategoryIdsAsync();
            bool isAvailable = product.IsActive && validCategoryIds.Contains(product.CategoryId) && (product.BrandId == null || (product.Brand != null && product.Brand.IsActive != false));

            return Ok(new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                ProductCode = product.ProductCode,
                Description = product.Description,
                BasePrice = product.BasePrice,
                OriginalPrice = product.OriginalPrice,
                TotalStock = product.TotalStock,
                ReservedStock = product.ReservedStock,
                AvailableStock = product.AvailableStock,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                BrandName = product.Brand != null ? product.Brand.Name : null,
                ThumbnailImage = product.ThumbnailImage,
                MainImage = product.MainImage,
                Images = product.Images,
                IsAvailable = isAvailable,
                BrandIsActive = product.Brand != null ? (bool?)product.Brand.IsActive : null,
                AverageRating = product.Reviews != null && product.Reviews.Any(r => !r.IsHidden) ? product.Reviews.Where(r => !r.IsHidden).Average(r => r.Rating) : 5.0,
                ReviewCount = product.Reviews != null ? product.Reviews.Count(r => !r.IsHidden) : 0
            });
        }

        // ================= CREATE: Cần đăng nhập =================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductRequest request)
        {
            // Kiểm tra Category có tồn tại không
            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
                return BadRequest("Category không tồn tại.");

            if (string.IsNullOrWhiteSpace(request.ProductCode))
            {
                request.ProductCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateProductCode(request.Name, 20);
            }
            if (await _context.Products.AnyAsync(p => p.ProductCode == request.ProductCode))
            {
                return BadRequest("Mã này đã tồn tại.");
            }

            var newProduct = new Product
            {
                Name = request.Name,
                Slug = request.Slug,
                ProductCode = request.ProductCode,
                Description = request.Description,
                BasePrice = request.BasePrice,
                OriginalPrice = request.OriginalPrice,
                TotalStock = request.TotalStock,
                ReservedStock = 0, // Mới tạo chưa có ai đặt hàng
                IsActive = request.IsActive,
                IsFeatured = request.IsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                ThumbnailImage = request.ThumbnailImage,
                MainImage = request.MainImage,
                Images = request.Images
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo sản phẩm thành công.", id = newProduct.Id });
        }

        // ================= UPDATE: Cần đăng nhập =================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Không tìm thấy sản phẩm.");


            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
                return BadRequest("Category không tồn tại.");

            if (string.IsNullOrWhiteSpace(request.ProductCode))
            {
                request.ProductCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateProductCode(request.Name, 20);
            }
            if (await _context.Products.AnyAsync(p => p.ProductCode == request.ProductCode && p.Id != id))
            {
                return BadRequest("Mã này đã tồn tại.");
            }

            product.Name = request.Name;
            product.Slug = request.Slug;
            product.ProductCode = request.ProductCode;
            product.Description = request.Description;
            product.BasePrice = request.BasePrice;
            product.OriginalPrice = request.OriginalPrice;
            
            // Cập nhật tồn kho (Trong thực tế nên dùng 1 API riêng cho Quản lý Tồn kho + Ghi Log)
            product.TotalStock = request.TotalStock;

            product.IsActive = request.IsActive;
            product.IsFeatured = request.IsFeatured;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.ThumbnailImage = request.ThumbnailImage;
            product.MainImage = request.MainImage;
            product.Images = request.Images;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật sản phẩm thành công.");
        }

        // ================= DELETE: Cần đăng nhập =================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Không tìm thấy sản phẩm.");

            // Đối với E-Commerce, KHÔNG NÊN xóa cứng Product vì nó liên quan đến Lịch sử Đơn hàng, Variant, Tồn kho.
            // Xóa cứng sẽ làm hỏng dữ liệu báo cáo.
            // Giải pháp tốt nhất là Xóa Mềm (Soft Delete) -> Đổi IsActive = false

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            
            // Tạm thời comment code xóa cứng lại để bảo toàn dữ liệu
            // _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Ok("Xóa sản phẩm thành công (Sản phẩm đã được ẩn / Xóa mềm).");
        }
    }
}
