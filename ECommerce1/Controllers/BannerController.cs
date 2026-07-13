using ECommerce.Models;
using ECommerce1.DTOs.Banner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BannerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task EnsureSeedDataAsync()
        {
            if (!await _context.Banners.AnyAsync())
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var defaultBanners = new List<Banner>
                {
                    // Published Banners
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-1.jpg", LinkUrl = "/khuyen-mai-1", Type = "Slider", IsActive = true, Position = 0, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-2.png", LinkUrl = "/khuyen-mai-2", Type = "Slider", IsActive = true, Position = 1, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-3.png", LinkUrl = "/khuyen-mai-3", Type = "Slider", IsActive = true, Position = 2, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-4.webp", LinkUrl = "/khuyen-mai-4", Type = "Slider", IsActive = true, Position = 3, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-5.png", LinkUrl = "/khuyen-mai-5", Type = "Slider", IsActive = true, Position = 4, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-6.png", LinkUrl = "/khuyen-mai-6", Type = "Slider", IsActive = true, Position = 5, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/top-banner.png", LinkUrl = "/khuyen-mai-hot", Type = "Top", IsActive = true, Position = 0, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-left.png", LinkUrl = "/khuyen-mai-trai", Type = "Left", IsActive = true, Position = 0, IsDraft = false },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-right.png", LinkUrl = "/khuyen-mai-phai", Type = "Right", IsActive = true, Position = 0, IsDraft = false },

                    // Draft Banners
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-1.jpg", LinkUrl = "/khuyen-mai-1", Type = "Slider", IsActive = true, Position = 0, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-2.png", LinkUrl = "/khuyen-mai-2", Type = "Slider", IsActive = true, Position = 1, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-3.png", LinkUrl = "/khuyen-mai-3", Type = "Slider", IsActive = true, Position = 2, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-4.webp", LinkUrl = "/khuyen-mai-4", Type = "Slider", IsActive = true, Position = 3, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-5.png", LinkUrl = "/khuyen-mai-5", Type = "Slider", IsActive = true, Position = 4, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-6.png", LinkUrl = "/khuyen-mai-6", Type = "Slider", IsActive = true, Position = 5, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/top-banner.png", LinkUrl = "/khuyen-mai-hot", Type = "Top", IsActive = true, Position = 0, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-left.png", LinkUrl = "/khuyen-mai-trai", Type = "Left", IsActive = true, Position = 0, IsDraft = true },
                    new Banner { ImageUrl = $"{baseUrl}/uploads/banners/banner-right.png", LinkUrl = "/khuyen-mai-phai", Type = "Right", IsActive = true, Position = 0, IsDraft = true }
                };

                _context.Banners.AddRange(defaultBanners);
                await _context.SaveChangesAsync();
            }
        }

        // GET: api/Banner
        [HttpGet]
        public async Task<IActionResult> GetPublishedActive()
        {
            await EnsureSeedDataAsync();
            var banners = await _context.Banners
                .Where(b => !b.IsDraft && b.IsActive)
                .OrderBy(b => b.Position)
                .ToListAsync();
            return Ok(banners);
        }

        // GET: api/Banner/draft
        [HttpGet("draft")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDraft()
        {
            await EnsureSeedDataAsync();
            var banners = await _context.Banners
                .Where(b => b.IsDraft)
                .OrderBy(b => b.Position)
                .ToListAsync();
            return Ok(banners);
        }

        // GET: api/Banner/published
        [HttpGet("published")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPublishedAdmin()
        {
            await EnsureSeedDataAsync();
            var banners = await _context.Banners
                .Where(b => !b.IsDraft)
                .OrderBy(b => b.Position)
                .ToListAsync();
            return Ok(banners);
        }

        // PUT: api/Banner/draft
        [HttpPut("draft")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDraft([FromBody] List<BannerDto> draftBanners)
        {
            if (draftBanners == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            await EnsureSeedDataAsync();

            var currentDrafts = await _context.Banners.Where(b => b.IsDraft).ToListAsync();
            _context.Banners.RemoveRange(currentDrafts);

            var newDrafts = draftBanners.Select((b, index) => new Banner
            {
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl ?? string.Empty,
                Type = b.Type,
                IsActive = b.IsActive,
                Position = b.Position,
                IsDraft = true,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Banners.AddRange(newDrafts);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Lưu bản nháp thành công!", Banners = newDrafts });
        }

        // POST: api/Banner/publish
        [HttpPost("publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish()
        {
            await EnsureSeedDataAsync();

            var currentPublished = await _context.Banners.Where(b => !b.IsDraft).ToListAsync();
            _context.Banners.RemoveRange(currentPublished);

            var currentDrafts = await _context.Banners.Where(b => b.IsDraft).ToListAsync();
            var newPublished = currentDrafts.Select(d => new Banner
            {
                ImageUrl = d.ImageUrl,
                LinkUrl = d.LinkUrl,
                Type = d.Type,
                IsActive = d.IsActive,
                Position = d.Position,
                IsDraft = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Banners.AddRange(newPublished);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xuất bản banner thành công!", Banners = newPublished });
        }

        // POST: api/Banner/discard
        [HttpPost("discard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Discard()
        {
            await EnsureSeedDataAsync();

            var currentDrafts = await _context.Banners.Where(b => b.IsDraft).ToListAsync();
            _context.Banners.RemoveRange(currentDrafts);

            var currentPublished = await _context.Banners.Where(b => !b.IsDraft).ToListAsync();
            var newDrafts = currentPublished.Select(p => new Banner
            {
                ImageUrl = p.ImageUrl,
                LinkUrl = p.LinkUrl,
                Type = p.Type,
                IsActive = p.IsActive,
                Position = p.Position,
                IsDraft = true,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Banners.AddRange(newDrafts);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã huỷ bỏ các thay đổi nháp và khôi phục về bản chính thức!", Banners = newDrafts });
        }
    }
}
