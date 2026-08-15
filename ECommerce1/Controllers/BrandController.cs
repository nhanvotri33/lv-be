// ==========================================================================
// MODULE: BrandController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module BrandController
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `GetAll` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? searchTerm = null)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
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

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
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

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(brands);
            }
        }

        // GET: api/Brand/5
        [HttpGet("{id}")]
        // [Hàm thực thi nghiệp vụ]: `GetById` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetById(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var brand = await _context.Brands.FindAsync(id);

            if (brand == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy thương hiệu.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var productsCount = await _context.Products.CountAsync(p => p.BrandId == id);

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
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
        // [Hàm thực thi nghiệp vụ]: `GetStats` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetStats(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == id);
            if (!brandExists)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy thương hiệu.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var totalActive = await _context.Products.CountAsync(p => p.BrandId == id && p.IsActive);
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var outOfStock = await _context.Products.CountAsync(p => p.BrandId == id && (p.TotalStock - p.ReservedStock) <= 0);
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
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

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
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
        // [Hàm thực thi nghiệp vụ]: `Create` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Create([FromBody] BrandRequest request)
        {
            if (!ModelState.IsValid)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(ModelState);
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var exists = await _context.Brands.AnyAsync(b => b.Slug == request.Slug);
            if (exists)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Slug đã tồn tại. Vui lòng chọn Slug khác.");
            }

            // =========================================================================
            // [XỬ LÝ MÃ THƯƠNG HIỆU - BACK-END]
            // - Tự động sinh mã thương hiệu (BrandCode) nếu bị trống.
            // - Kiểm tra tính duy nhất (Uniqueness Constraint) để tránh trùng lặp thương hiệu.
            // =========================================================================
                
            if (string.IsNullOrWhiteSpace(request.BrandCode))
            {
                request.BrandCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateBrandOrCategoryCode(request.Name, 10);
            }
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            if (await _context.Brands.AnyAsync(b => b.BrandCode == request.BrandCode))
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
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

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Brands.Add(brand);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
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
        // [Hàm thực thi nghiệp vụ]: `Update` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequest request)
        {
            if (!ModelState.IsValid)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(ModelState);
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy thương hiệu.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var exists = await _context.Brands.AnyAsync(b => b.Slug == request.Slug && b.Id != id);
            if (exists)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Slug đã tồn tại. Vui lòng chọn Slug khác.");
            }

            if (string.IsNullOrWhiteSpace(request.BrandCode))
            {
                request.BrandCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateBrandOrCategoryCode(request.Name, 10);
            }
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            if (await _context.Brands.AnyAsync(b => b.BrandCode == request.BrandCode && b.Id != id))
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
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

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Cập nhật thương hiệu thành công." });
        }

        // DELETE: api/Brand/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Delete` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (brand == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy thương hiệu.");
            }

            if (brand.Products != null && brand.Products.Any())
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Không thể xóa thương hiệu đang có sản phẩm. Vui lòng xóa sản phẩm trước hoặc đổi thương hiệu cho sản phẩm.");
            }

            if (!string.IsNullOrEmpty(brand.ImageUrl))
            {
                _fileService.DeleteImage(brand.ImageUrl);
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Brands.Remove(brand);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Xóa thương hiệu thành công." });
        }
    }
}
