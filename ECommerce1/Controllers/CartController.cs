using ECommerce.Models;
using ECommerce1.DTOs.Cart;
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
    [Authorize] // Tất cả tính năng giỏ hàng đều yêu cầu đăng nhập
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng của user hiện tại
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            // Tìm giỏ hàng của User
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // Nếu user chưa có giỏ hàng, tự động tạo mới
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Map ra DTO
            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems?.Select(ci => new CartItemResponse
                {
                    Id = ci.Id,
                    VariantId = ci.VariantId,
                    ProductName = ci.ProductVariant?.Product?.Name ?? "Sản phẩm đã xóa",
                    VariantName = ci.ProductVariant?.Name ?? "Biến thể đã xóa",
                    ImageUrl = ci.ProductVariant?.ImageId ?? ci.ProductVariant?.Product?.ThumbnailImage,
                    Price = ci.ProductVariant?.Price ?? 0,
                    Quantity = ci.Quantity
                }).ToList() ?? new System.Collections.Generic.List<CartItemResponse>()
            };

            return Ok(response);
        }

        // Làm sạch toàn bộ giỏ hàng
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok("Đã làm sạch giỏ hàng.");
        }
    }
}
