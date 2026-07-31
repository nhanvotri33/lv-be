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
    [Authorize] // Bảo mật API
    public class CartItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= THÊM VÀO GIỎ HÀNG =================
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest("Số lượng phải lớn hơn 0.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra & Khóa Variant với UPDLOCK để tránh race condition khi spam click thêm vào giỏ
                var variant = await _context.ProductVariants
                    .FromSqlRaw("SELECT * FROM ProductVariants WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0}", request.VariantId)
                    .FirstOrDefaultAsync();

                if (variant == null)
                    return NotFound("Sản phẩm hoặc biến thể không tồn tại.");

                // 2. Kiểm tra tồn kho
                if (variant.AvailableStock < request.Quantity)
                    return BadRequest("Số lượng tồn kho không đủ để thêm vào giỏ.");

                // 3. Tìm hoặc tạo giỏ hàng cho User
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync(); // Phải lưu xuống db để có Cart.Id
                }

                // 4. Kiểm tra xem Variant này đã có trong giỏ hàng chưa
                var existingItem = cart.CartItems?.FirstOrDefault(ci => ci.VariantId == request.VariantId);

                if (existingItem != null)
                {
                    // Đã có -> Cộng dồn số lượng
                    if (existingItem.Quantity + request.Quantity > variant.AvailableStock)
                        return BadRequest("Tổng số lượng trong giỏ hàng đã vượt quá tồn kho hiện tại.");
                        
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    // Chưa có -> Tạo mới
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        VariantId = request.VariantId,
                        Quantity = request.Quantity,
                        AppliedCampaignId = request.AppliedCampaignId,
                        ParentCartItemId = request.ParentCartItemId,
                        IsAddon = request.IsAddon
                    };
                    _context.CartItems.Add(newItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Đã thêm sản phẩm vào giỏ hàng.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi xử lý giỏ hàng: {ex.Message}");
            }
        }

        // ================= CẬP NHẬT SỐ LƯỢNG =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int newQuantity)
        {
            if (newQuantity <= 0)
                return BadRequest("Số lượng phải lớn hơn 0. Nếu muốn xóa sản phẩm hãy dùng API DELETE.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(ci => ci.Id == id);

                if (cartItem == null)
                    return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

                // Kiểm tra bảo mật: Không được sửa giỏ hàng của người khác
                if (cartItem.Cart.UserId != userId)
                    return StatusCode(403, "Bạn không có quyền sửa giỏ hàng của người khác.");

                // Lock ProductVariant bằng UPDLOCK
                var variant = await _context.ProductVariants
                    .FromSqlRaw("SELECT * FROM ProductVariants WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0}", cartItem.VariantId)
                    .FirstOrDefaultAsync();

                if (variant == null || variant.AvailableStock < newQuantity)
                    return BadRequest("Số lượng tồn kho không đủ.");

                cartItem.Quantity = newQuantity;
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Cập nhật số lượng thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi cập nhật giỏ hàng: {ex.Message}");
            }
        }

        // ================= XÓA SẢN PHẨM KHỎI GIỎ HÀNG =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
                return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

            // Kiểm tra bảo mật
            if (cartItem.Cart.UserId != userId)
                return StatusCode(403, "Bạn không có quyền thao tác trên giỏ hàng của người khác.");

            _context.CartItems.Remove(cartItem);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Đã xóa sản phẩm khỏi giỏ hàng.");
        }
    }
}
