// ==========================================================================
// MODULE: NotificationController.cs
// MỤC ĐÍCH: Controller xử lý danh sách thông báo (UserNotification)
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.Notification;
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
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);
            return userId;
        }

        // 1. Lấy danh sách thông báo của người dùng + unreadCount
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Product)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30) // Lấy tối đa 30 thông báo mới nhất
                .ToListAsync();

            int unreadCount = await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            var items = notifications.Select(n => new UserNotificationResponse
            {
                Id = n.Id,
                ProductId = n.ProductId,
                ProductSlug = n.Product?.Slug,
                ProductImage = n.Product?.ThumbnailImage,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

            return Ok(new NotificationListResponse
            {
                UnreadCount = unreadCount,
                Items = items
            });
        }

        // 2. Đánh dấu 1 thông báo là đã đọc
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null) return NotFound(new { message = "Không tìm thấy thông báo." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc." });
        }

        // 3. Đánh dấu tất cả thông báo là đã đọc
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var unreadNotifications = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu tất cả thông báo là đã đọc." });
        }
    }
}
