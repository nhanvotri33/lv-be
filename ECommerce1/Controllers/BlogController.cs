using ECommerce.Models;
using ECommerce1.DTOs.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        //// GET: api/Blog
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] bool? isFeatured = null,
            [FromQuery] bool? isPublished = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Blogs.Include(b => b.User).AsQueryable();

            // Nếu người dùng không chỉ định isPublished, mặc định khách chỉ xem bài đã xuất bản
            if (isPublished.HasValue)
            {
                query = query.Where(b => b.IsPublished == isPublished.Value);
            }
            else if (!User.IsInRole("Admin"))
            {
                query = query.Where(b => b.IsPublished);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(term)
                                      || (b.Summary != null && b.Summary.ToLower().Contains(term))
                                      || (b.Tags != null && b.Tags.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category != null && b.Category.ToLower() == category.Trim().ToLower());
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(b => b.IsFeatured == isFeatured.Value);
            }

            int totalItems = await query.CountAsync();
            int page = pageNumber < 1 ? 1 : pageNumber;
            int size = pageSize < 1 ? 10 : pageSize;
            int totalPages = (int)Math.Ceiling((double)totalItems / size);

            var items = await query
                .OrderByDescending(b => b.IsFeatured)
                .ThenByDescending(b => b.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(b => BlogControllerHelpers.MapToResponse(b))
                .ToListAsync();

            return Ok(new
            {
                items,
                totalItems,
                pageNumber = page,
                pageSize = size,
                totalPages
            });
        }

        // GET: api/Blog/{id} lấy data từ id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var blog = await _context.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            // Tăng số lượt xem bài viết
            blog.ViewCount += 1;
            await _context.SaveChangesAsync();

            return Ok(BlogControllerHelpers.MapToResponse(blog));
        }

        // GET: api/Blog/slug/{slug} lấy data từ slug
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var blog = await _context.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Slug == slug);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            // Tăng số lượt xem bài viết
            blog.ViewCount += 1;
            await _context.SaveChangesAsync();

            return Ok(BlogControllerHelpers.MapToResponse(blog));
        }

        // POST: api/Blog
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] BlogRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var slug = string.IsNullOrWhiteSpace(request.Slug) ? BlogControllerHelpers.GenerateSlug(request.Title) : request.Slug.Trim();

            // Kiểm tra trùng slug
            if (await _context.Blogs.AnyAsync(b => b.Slug == slug))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";
            }

            Guid? userId = null;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid parsedId))
            {
                userId = parsedId;
            }

            var blog = new Blog
            {
                Title = request.Title.Trim(),
                Slug = slug,
                Summary = request.Summary?.Trim(),
                Content = request.Content,
                ThumbnailUrl = request.ThumbnailUrl?.Trim(),
                Author = request.Author?.Trim(),
                Category = request.Category?.Trim(),
                Tags = request.Tags?.Trim(),
                IsPublished = request.IsPublished,
                IsFeatured = request.IsFeatured,
                ViewCount = 0,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBySlug), new { slug = blog.Slug }, BlogControllerHelpers.MapToResponse(blog));
        }

        // PUT: api/Blog/{id} cập nhật từ id
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var blog = await _context.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            var slug = string.IsNullOrWhiteSpace(request.Slug) ? BlogControllerHelpers.GenerateSlug(request.Title) : request.Slug.Trim();
            if (await _context.Blogs.AnyAsync(b => b.Slug == slug && b.Id != id))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";
            }

            blog.Title = request.Title.Trim();
            blog.Slug = slug;
            blog.Summary = request.Summary?.Trim();
            blog.Content = request.Content;
            blog.ThumbnailUrl = request.ThumbnailUrl?.Trim();
            blog.Author = request.Author?.Trim();
            blog.Category = request.Category?.Trim();
            blog.Tags = request.Tags?.Trim();
            blog.IsPublished = request.IsPublished;
            blog.IsFeatured = request.IsFeatured;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(BlogControllerHelpers.MapToResponse(blog));
        }

        // PUT: api/Blog/{slug} thêm data từ slug
        [HttpPut("slug/{slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBySlug(string slug, [FromBody] BlogRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var blog = await _context.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Slug == slug);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            var newSlug = string.IsNullOrWhiteSpace(request.Slug) ? BlogControllerHelpers.GenerateSlug(request.Title) : request.Slug.Trim();
            if (await _context.Blogs.AnyAsync(b => b.Slug == slug && b.Id != blog.Id))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";
            }

            blog.Title = request.Title.Trim();
            blog.Slug = slug;
            blog.Summary = request.Summary?.Trim();
            blog.Content = request.Content;
            blog.ThumbnailUrl = request.ThumbnailUrl?.Trim();
            blog.Author = request.Author?.Trim();
            blog.Category = request.Category?.Trim();
            blog.Tags = request.Tags?.Trim();
            blog.IsPublished = request.IsPublished;
            blog.IsFeatured = request.IsFeatured;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(BlogControllerHelpers.MapToResponse(blog));
        }

        // DELETE: api/Blog/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa bài viết thành công." });
        }


        // PATCH: api/Blog/{id}/toggle-publish
        [HttpPatch("{id:int}/toggle-publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            blog.IsPublished = !blog.IsPublished;
            blog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã {(blog.IsPublished ? "xuất bản" : "ẩn")} bài viết.", isPublished = blog.IsPublished });
        }

        // PATCH: api/Blog/{slug}/
        [HttpPatch("slug/{slug}/toggle-publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePublish(string slug)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
            if (blog == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            blog.IsPublished = !blog.IsPublished;
            blog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã {(blog.IsPublished ? "xuất bản" : "ẩn")} bài viết.", isPublished = blog.IsPublished });
        }
    }
}
