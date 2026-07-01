using ECommerce.Models;
using ECommerce1.DTOs.Brand;
using ECommerce1.Services;
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
    public class BrandController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public BrandController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/Brand
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? searchTerm = null)
        {
            var query = _context.Brands.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(term) || b.BrandCode.ToLower().Contains(term));
            }

            if (pageNumber.HasValue && pageNumber.Value > 0)
            {
                int size = pageSize ?? 10;
                var totalItems = await query.CountAsync();
                
                var brands = await query
                    .OrderByDescending(b => b.Id)
                    .Skip((pageNumber.Value - 1) * size)
                    .Take(size)
                    .Select(b => new BrandResponse
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Slug = b.Slug,
                        BrandCode = b.BrandCode,
                        Description = b.Description,
                        ImageUrl = b.ImageUrl,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        ProductsCount = b.Products.Count()
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalItems / size);

                return Ok(new
                {
                    items = brands,
                    totalItems = totalItems,
                    pageNumber = pageNumber.Value,
                    pageSize = size,
                    totalPages = totalPages
                });
            }
            else
            {
                var brands = await query
                    .OrderByDescending(b => b.Id)
                    .Select(b => new BrandResponse
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Slug = b.Slug,
                        BrandCode = b.BrandCode,
                        Description = b.Description,
                        ImageUrl = b.ImageUrl,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        ProductsCount = b.Products.Count()
                    })
                    .ToListAsync();

                return Ok(brands);
            }
        }

        // GET: api/Brand/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand == null)
            {
                return NotFound("Không tìm thấy thương hiệu.");
            }

            var productsCount = await _context.Products.CountAsync(p => p.BrandId == id);

            return Ok(new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Slug = brand.Slug,
                BrandCode = brand.BrandCode,
                Description = brand.Description,
                ImageUrl = brand.ImageUrl,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt,
                ProductsCount = productsCount
            });
        }

        // GET: api/Brand/5/stats
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == id);
            if (!brandExists)
            {
                return NotFound("Không tìm thấy thương hiệu.");
            }

            var totalActive = await _context.Products.CountAsync(p => p.BrandId == id && p.IsActive);
            var outOfStock = await _context.Products.CountAsync(p => p.BrandId == id && (p.TotalStock - p.ReservedStock) <= 0);
            var totalStock = await _context.Products.Where(p => p.BrandId == id).SumAsync(p => (int?)p.TotalStock) ?? 0;

            var topSellers = await _context.Products
                .Where(p => p.BrandId == id)
                .Select(p => new
                {
                    p.Name,
                    p.ThumbnailImage,
                    SalesCount = p.ProductVariants.SelectMany(v => v.OrderItems).Sum(oi => (int?)oi.Quantity) ?? 0
                })
                .OrderByDescending(p => p.SalesCount)
                .Take(3)
                .Select(p => new
                {
                    p.Name,
                    p.ThumbnailImage
                })
                .ToListAsync();

            return Ok(new
            {
                totalActive,
                outOfStock,
                totalStock,
                topSellers
            });
        }

        // POST: api/Brand
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] BrandRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _context.Brands.AnyAsync(b => b.Slug == request.Slug);
            if (exists)
            {
                return BadRequest("Slug đã tồn tại. Vui lòng chọn Slug khác.");
            }

            if (string.IsNullOrWhiteSpace(request.BrandCode))
            {
                request.BrandCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateBrandOrCategoryCode(request.Name, 10);
            }
            if (await _context.Brands.AnyAsync(b => b.BrandCode == request.BrandCode))
            {
                return BadRequest("Mã này đã tồn tại.");
            }

            var brand = new Brand
            {
                Name = request.Name,
                Slug = request.Slug,
                BrandCode = request.BrandCode,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = brand.Id }, new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Slug = brand.Slug,
                BrandCode = brand.BrandCode,
                Description = brand.Description,
                ImageUrl = brand.ImageUrl,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt
            });
        }

        // PUT: api/Brand/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound("Không tìm thấy thương hiệu.");
            }

            var exists = await _context.Brands.AnyAsync(b => b.Slug == request.Slug && b.Id != id);
            if (exists)
            {
                return BadRequest("Slug đã tồn tại. Vui lòng chọn Slug khác.");
            }

            if (string.IsNullOrWhiteSpace(request.BrandCode))
            {
                request.BrandCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateBrandOrCategoryCode(request.Name, 10);
            }
            if (await _context.Brands.AnyAsync(b => b.BrandCode == request.BrandCode && b.Id != id))
            {
                return BadRequest("Mã này đã tồn tại.");
            }

            if (brand.ImageUrl != request.ImageUrl)
            {
                _fileService.DeleteImage(brand.ImageUrl);
            }

            brand.Name = request.Name;
            brand.Slug = request.Slug;
            brand.BrandCode = request.BrandCode;
            brand.Description = request.Description;
            brand.ImageUrl = request.ImageUrl;
            brand.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thương hiệu thành công." });
        }

        // DELETE: api/Brand/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (brand == null)
            {
                return NotFound("Không tìm thấy thương hiệu.");
            }

            if (brand.Products != null && brand.Products.Any())
            {
                return BadRequest("Không thể xóa thương hiệu đang có sản phẩm. Vui lòng xóa sản phẩm trước hoặc đổi thương hiệu cho sản phẩm.");
            }

            if (!string.IsNullOrEmpty(brand.ImageUrl))
            {
                _fileService.DeleteImage(brand.ImageUrl);
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thương hiệu thành công." });
        }
    }
}
