// ==========================================================================
// MODULE: BannerController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module BannerController
// ==========================================================================
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

        // GET: api/Banner
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetPublishedActive` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetPublishedActive()
        {
            var banners = await _context.Banners
                .Where(b => !b.IsDraft && b.IsActive)
                .OrderBy(b => b.Position)
                .ToListAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(banners);
        }

        // GET: api/Banner/draft
        [HttpGet("draft")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetDraft` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetDraft()
        {
            var banners = await _context.Banners
                .Where(b => b.IsDraft)
                .OrderBy(b => b.Position)
                .ToListAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(banners);
        }

        // GET: api/Banner/published
        [HttpGet("published")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetPublishedAdmin` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetPublishedAdmin()
        {
            var banners = await _context.Banners
                .Where(b => !b.IsDraft)
                .OrderBy(b => b.Position)
                .ToListAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(banners);
        }

        // PUT: api/Banner/draft
        [HttpPut("draft")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `UpdateDraft` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdateDraft([FromBody] List<BannerDto> draftBanners)
        {
            if (draftBanners == null)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var currentDrafts = await _context.Banners.Where(b => b.IsDraft).ToListAsync();
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
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

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Banners.AddRange(newDrafts);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { Message = "Lưu bản nháp thành công!", Banners = newDrafts });
        }

        // POST: api/Banner/publish
        [HttpPost("publish")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Publish` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Publish()
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var currentPublished = await _context.Banners.Where(b => !b.IsDraft).ToListAsync();
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Banners.RemoveRange(currentPublished);

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
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

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Banners.AddRange(newPublished);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { Message = "Xuất bản banner thành công!", Banners = newPublished });
        }

        // POST: api/Banner/discard
        [HttpPost("discard")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Discard` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Discard()
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var currentDrafts = await _context.Banners.Where(b => b.IsDraft).ToListAsync();
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Banners.RemoveRange(currentDrafts);

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
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

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Banners.AddRange(newDrafts);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { Message = "Đã huỷ bỏ các thay đổi nháp và khôi phục về bản chính thức!", Banners = newDrafts });
        }
    }
}
