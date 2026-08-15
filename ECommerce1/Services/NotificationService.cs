// ==========================================================================
// MODULE: NotificationService.cs
// MỤC ĐÍCH: Service hỗ trợ phát hiện sự kiện Giảm giá / Có hàng lại và gửi thông báo
// ==========================================================================
using ECommerce.Models;
using ECommerce1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Xử lý khi giá sản phẩm/biến thể giảm
        public async Task NotifyPriceDropAsync(int productId, decimal oldPrice, decimal newPrice)
        {
            if (newPrice >= oldPrice || oldPrice <= 0) return;

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // Tìm các user có lưu sản phẩm này trong Wishlist và bật NotifyOnPriceDrop
            var interestedUsers = await _context.Wishlists
                .Where(w => w.ProductId == productId && w.NotifyOnPriceDrop)
                .Select(w => w.UserId)
                .Distinct()
                .ToListAsync();

            if (!interestedUsers.Any()) return;

            decimal dropAmount = oldPrice - newPrice;
            string formattedDrop = dropAmount.ToString("N0") + "đ";
            string formattedNew = newPrice.ToString("N0") + "đ";

            foreach (var userId in interestedUsers)
            {
                var notification = new UserNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    Title = $"🎉 Sản phẩm [{product.Name}] vừa giảm giá!",
                    Message = $"Sản phẩm bạn yêu thích vừa giảm giá {formattedDrop}. Giá mới chỉ còn {formattedNew}!",
                    Type = "PriceDrop",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserNotifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        // 2. Xử lý khi sản phẩm hết hàng nay có hàng lại (Restock)
        public async Task NotifyRestockAsync(int productId, int oldStock, int newStock)
        {
            if (oldStock > 0 || newStock <= 0) return;

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // Tìm các user có lưu sản phẩm này trong Wishlist và bật NotifyOnRestock
            var interestedUsers = await _context.Wishlists
                .Where(w => w.ProductId == productId && w.NotifyOnRestock)
                .Select(w => w.UserId)
                .Distinct()
                .ToListAsync();

            if (!interestedUsers.Any()) return;

            foreach (var userId in interestedUsers)
            {
                var notification = new UserNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    Title = $"📦 Sản phẩm [{product.Name}] đã có hàng trở lại!",
                    Message = $"Sản phẩm trong danh sách yêu thích của bạn vừa được nạp lại kho ({newStock} sản phẩm). Nhanh tay đặt mua ngay!",
                    Type = "Restock",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserNotifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }
    }
}
