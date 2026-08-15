// ==========================================================================
// MODULE: WishlistController.cs
// MỤC ĐÍCH: Controller xử lý danh sách sản phẩm yêu thích (Wishlist)
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.Wishlist;
using ECommerce1.Models;
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
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out Guid userId);
            return userId;
        }

        // 1. Lấy danh sách sản phẩm yêu thích của user đang đăng nhập
        [HttpGet]
        public async Task<IActionResult> GetMyWishlist()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var wishlists = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductVariants)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            var response = wishlists.Select(w =>
            {
                var variants = w.Product.ProductVariants ?? new System.Collections.Generic.List<ProductVariant>();
                decimal basePrice = w.Product.BasePrice;
                decimal minPrice = variants.Any() ? variants.Min(v => v.Price) : basePrice;
                decimal maxPrice = variants.Any() ? variants.Max(v => v.Price) : basePrice;
                int totalStock = variants.Any() ? variants.Sum(v => v.TotalStock) : 0;

                return new WishlistResponse
                {
                    Id = w.Id,
                    ProductId = w.ProductId,
                    ProductName = w.Product.Name,
                    ProductSlug = w.Product.Slug,
                    ProductImage = w.Product.ThumbnailImage,
                    BasePrice = basePrice,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    TotalStock = totalStock,
                    NotifyOnPriceDrop = w.NotifyOnPriceDrop,
                    NotifyOnRestock = w.NotifyOnRestock,
                    CreatedAt = w.CreatedAt
                };
            }).ToList();

            return Ok(response);
        }

        // 2. Thêm hoặc bỏ sản phẩm khỏi Wishlist (Toggle)
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleWishlist([FromBody] ToggleWishlistRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });

            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId);

            bool isSaved;
            if (existingItem != null)
            {
                _context.Wishlists.Remove(existingItem);
                isSaved = false;
            }
            else
            {
                var newWishlist = new Wishlist
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = request.ProductId,
                    NotifyOnPriceDrop = true,
                    NotifyOnRestock = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Wishlists.Add(newWishlist);
                isSaved = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                isSaved,
                message = isSaved ? "Đã thêm sản phẩm vào danh sách yêu thích." : "Đã xóa sản phẩm khỏi danh sách yêu thích."
            });
        }

        // 3. Cập nhật cài đặt thông báo (NotifyOnPriceDrop & NotifyOnRestock)
        [HttpPut("notification-settings")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateWishlistNotificationRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId);

            if (wishlistItem == null)
            {
                return NotFound(new { message = "Sản phẩm chưa có trong danh sách yêu thích của bạn." });
            }

            wishlistItem.NotifyOnPriceDrop = request.NotifyOnPriceDrop;
            wishlistItem.NotifyOnRestock = request.NotifyOnRestock;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật cài đặt thông báo thành công." });
        }

        // 4. Kiểm tra trạng thái yêu thích của 1 sản phẩm
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckWishlistStatus(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Ok(new { isSaved = false, notifyOnPriceDrop = false, notifyOnRestock = false });
            }

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null)
            {
                return Ok(new { isSaved = false, notifyOnPriceDrop = false, notifyOnRestock = false });
            }

            return Ok(new
            {
                isSaved = true,
                notifyOnPriceDrop = wishlistItem.NotifyOnPriceDrop,
                notifyOnRestock = wishlistItem.NotifyOnRestock
            });
        }
    }
}
