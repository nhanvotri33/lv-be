using ECommerce.Models;
using ECommerce1.DTOs.Category;
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
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
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

        // ================= READ: Ai cũng xem được (Hoặc User) =================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? isRoot = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .AsQueryable();

            if (isRoot == true)
            {
                query = query.Where(c => c.ParentId == null);
            }

            if (!includeInactive)
            {
                var validCategoryIds = await GetValidCategoryIdsAsync();
                query = query.Where(c => validCategoryIds.Contains(c.Id));
            }

            var categories = await query
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    CategoryCode = c.CategoryCode,
                    Description = c.Description,
                    IconUrl = c.IconUrl,
                    ParentId = c.ParentId,
                    Level = c.ParentId == null ? 1 : (c.ParentCategory.ParentId == null ? 2 : 3),
                    SubCategoriesCount = c.SubCategories.Count,
                    ProductsCount = c.Products.Count,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    SpecsTemplate = c.SpecsTemplate
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] bool includeInactive = false)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Không tìm thấy danh mục.");

            if (!includeInactive)
            {
                var validCategoryIds = await GetValidCategoryIdsAsync();
                if (!validCategoryIds.Contains(id))
                {
                    return NotFound("Danh mục này không tồn tại hoặc đã bị ẩn.");
                }
            }

            return Ok(new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IconUrl = category.IconUrl,
                ParentId = category.ParentId,
                Level = category.ParentId == null ? 1 : (category.ParentCategory.ParentId == null ? 2 : 3),
                SubCategoriesCount = category.SubCategories?.Count ?? 0,
                ProductsCount = category.Products?.Count ?? 0,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                SpecsTemplate = category.SpecsTemplate
            });
        }

        // GET: api/Category/5/details
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetCategoryDetails(int id, [FromQuery] bool includeInactive = false)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Products)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Không tìm thấy danh mục.");

            if (!includeInactive)
            {
                var validCategoryIds = await GetValidCategoryIdsAsync();
                if (!validCategoryIds.Contains(id))
                {
                    return NotFound("Danh mục này không tồn tại hoặc đã bị ẩn.");
                }

                var subCategories = category.SubCategories
                    .Where(c => validCategoryIds.Contains(c.Id))
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        CategoryCode = c.CategoryCode,
                        Description = c.Description,
                        IconUrl = c.IconUrl,
                        ParentId = c.ParentId,
                        Level = c.ParentId == null ? 1 : (category.ParentId == null ? 2 : 3),
                        SubCategoriesCount = c.SubCategories?.Count ?? 0,
                        ProductsCount = c.Products?.Count ?? 0,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        SpecsTemplate = c.SpecsTemplate
                    }).ToList();

                return Ok(new
                {
                    SubCategories = subCategories
                });
            }
            else
            {
                var subCategories = category.SubCategories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    CategoryCode = c.CategoryCode,
                    Description = c.Description,
                    IconUrl = c.IconUrl,
                    ParentId = c.ParentId,
                    Level = c.ParentId == null ? 1 : (category.ParentId == null ? 2 : 3),
                    SubCategoriesCount = c.SubCategories?.Count ?? 0,
                    ProductsCount = c.Products?.Count ?? 0,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    SpecsTemplate = c.SpecsTemplate
                }).ToList();

                return Ok(new
                {
                    SubCategories = subCategories
                });
            }
        }

        // ================= CREATE: Chỉ Admin =================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryCode))
            {
                request.CategoryCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateBrandOrCategoryCode(request.Name, 20);
            }
            if (await _context.Categories.AnyAsync(c => c.CategoryCode == request.CategoryCode))
            {
                return BadRequest("Mã này đã tồn tại.");
            }

            var newCategory = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                CategoryCode = request.CategoryCode,
                Description = request.Description,
                IconUrl = request.IconUrl,
                MetaTitle = request.MetaTitle,
                MetaDescription = request.MetaDescription,
                ParentId = request.ParentId,
                IsActive = request.IsActive,
                SpecsTemplate = request.SpecsTemplate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return Ok("Tạo danh mục thành công.");
        }

        // ================= UPDATE: Chỉ Admin =================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Không tìm thấy danh mục.");

            // 1. Luồng 1: Chặn "Tự nhận mình làm cha"
            if (request.ParentId.HasValue && request.ParentId.Value == id)
            {
                return BadRequest("Không thể chọn chính danh mục này làm danh mục cha.");
            }

            // 2. Luồng 2: Chặn "Nghịch lý thời gian" (Vòng lặp gia phả)
            if (request.ParentId.HasValue)
            {
                int? currentAncestorId = request.ParentId.Value;
                while (currentAncestorId.HasValue)
                {
                    if (currentAncestorId.Value == id)
                    {
                        return BadRequest("Lỗi vòng lặp gia phả: Không thể chọn danh mục con/cháu của chính nó làm danh mục cha.");
                    }
                    var parentOfAncestor = await _context.Categories
                        .Where(c => c.Id == currentAncestorId.Value)
                        .Select(c => c.ParentId)
                        .FirstOrDefaultAsync();
                    currentAncestorId = parentOfAncestor;
                }
            }

            // 3. Luồng 3: Chặn "Tràn cấp độ" (Over-depth)
            int newParentLevel = 0;
            if (request.ParentId.HasValue)
            {
                var parentCat = await _context.Categories
                    .Include(p => p.ParentCategory)
                    .FirstOrDefaultAsync(c => c.Id == request.ParentId.Value);
                if (parentCat == null)
                {
                    return BadRequest("Không tìm thấy danh mục cha mới.");
                }
                newParentLevel = parentCat.ParentId == null ? 1 : (parentCat.ParentCategory.ParentId == null ? 2 : 3);
            }

            // Tính độ sâu của cây danh mục con thuộc category hiện tại
            var childIds = await _context.Categories
                .Where(c => c.ParentId == id)
                .Select(c => c.Id)
                .ToListAsync();

            int subTreeDepth = 1; // Bản thân nó
            if (childIds.Any())
            {
                subTreeDepth = 2; // Có con
                var hasGrandChildren = await _context.Categories
                    .AnyAsync(c => c.ParentId.HasValue && childIds.Contains(c.ParentId.Value));
                if (hasGrandChildren)
                {
                    subTreeDepth = 3; // Có cháu
                }
            }

            if (newParentLevel + subTreeDepth > 3)
            {
                return BadRequest($"Không thể thay đổi danh mục cha vì tổng số cấp phân cấp sẽ vượt quá giới hạn (tối đa 3 cấp). Danh mục hiện tại cùng các con cháu có độ sâu {subTreeDepth} cấp và danh mục cha mới ở cấp {newParentLevel}.");
            }

            if (string.IsNullOrWhiteSpace(request.CategoryCode))
            {
                request.CategoryCode = ECommerce1.Helpers.CodeGeneratorHelper.GenerateBrandOrCategoryCode(request.Name, 20);
            }
            if (await _context.Categories.AnyAsync(c => c.CategoryCode == request.CategoryCode && c.Id != id))
            {
                return BadRequest("Mã này đã tồn tại.");
            }

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.CategoryCode = request.CategoryCode;
            category.Description = request.Description;
            category.IconUrl = request.IconUrl;
            category.MetaTitle = request.MetaTitle;
            category.MetaDescription = request.MetaDescription;
            category.ParentId = request.ParentId;
            category.IsActive = request.IsActive;
            category.SpecsTemplate = request.SpecsTemplate;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật danh mục thành công.");
        }

        // ================= DELETE: Chỉ Admin =================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products) // Kiểm tra xem có Product nào đang dùng không
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Không tìm thấy danh mục.");

            if (category.Products != null && category.Products.Any())
                return BadRequest("Không thể xóa vì danh mục này đang chứa sản phẩm.");

            if (await _context.Categories.AnyAsync(c => c.ParentId == id))
                return BadRequest("Không thể xóa vì danh mục này có chứa danh mục con.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok("Xóa danh mục thành công.");
        }
    }
}