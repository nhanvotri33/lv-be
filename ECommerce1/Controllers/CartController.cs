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

            // Lấy danh sách item
            var items = cart.CartItems?.ToList() ?? new System.Collections.Generic.List<CartItem>();
            var responseItems = new System.Collections.Generic.List<CartItemResponse>();

            bool dbChanged = false;

            foreach (var item in items)
            {
                decimal price = item.ProductVariant?.Price ?? 0;

                if (item.AppliedComboId.HasValue)
                {
                    int comboId = item.AppliedComboId.Value;
                    // Lấy cấu hình combo
                    var comboConfig = await _context.ProductComboItems.Where(c => c.ProductComboId == comboId).ToListAsync();

                    var mainProductIds = comboConfig.Where(c => c.IsMain).Select(c => c.ProductId).ToList();

                    // Check if cart has main product for this combo
                    bool hasMain = items.Any(ci => ci.AppliedComboId == comboId && ci.ProductVariant != null && mainProductIds.Contains(ci.ProductVariant.ProductId));

                    if (hasMain)
                    {
                        var config = comboConfig.FirstOrDefault(c => c.ProductId == item.ProductVariant?.ProductId);
                        if (config != null && !config.IsMain)
                        {
                            if (config.DiscountType == "Percentage")
                                price = price * (1 - config.DiscountValue / 100);
                            else if (config.DiscountType == "FixedAmount")
                                price = Math.Max(0, price - config.DiscountValue);
                        }
                    }
                    else
                    {
                        // Không có sản phẩm chính -> Hủy combo
                        item.AppliedComboId = null;
                        _context.CartItems.Update(item);
                        dbChanged = true;
                    }
                }

                responseItems.Add(new CartItemResponse
                {
                    Id = item.Id,
                    VariantId = item.VariantId,
                    ProductName = item.ProductVariant?.Product?.Name ?? "Sản phẩm đã xóa",
                    VariantName = item.ProductVariant?.Name ?? "Biến thể đã xóa",
                    ImageUrl = item.ProductVariant?.ImageId ?? item.ProductVariant?.Product?.ThumbnailImage,
                    Price = price,
                    Quantity = item.Quantity,
                    AppliedComboId = item.AppliedComboId
                });
            }

            if (dbChanged)
            {
                await _context.SaveChangesAsync();
            }

            // Map ra DTO
            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = responseItems
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

        // Đồng bộ giỏ hàng hàng loạt (Batch Sync)
        [HttpPost("sync")]
        public async Task<IActionResult> SyncCart([FromBody] System.Collections.Generic.List<SyncCartRequest> items)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

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

            // Xóa sạch các item cũ
            if (cart.CartItems != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
            }

            // Thêm các item mới
            if (items != null)
            {
                foreach (var item in items)
                {
                    // Kiểm tra xem variant có tồn tại không
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                    if (variant == null) continue;

                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        AppliedComboId = item.AppliedComboId
                    };
                    _context.CartItems.Add(cartItem);
                }
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Đồng bộ giỏ hàng thành công.");
        }
    }

    public class SyncCartRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public int? AppliedComboId { get; set; }
    }
}
