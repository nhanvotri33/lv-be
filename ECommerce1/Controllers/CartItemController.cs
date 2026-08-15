// ==========================================================================
// MODULE: CartItemController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module CartItemController
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `AddToCart` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            if (request.Quantity <= 0)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Số lượng phải lớn hơn 0.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra & Khóa Variant với UPDLOCK để tránh race condition khi spam click thêm vào giỏ
                var variant = await _context.ProductVariants
                    .FromSqlRaw("SELECT * FROM ProductVariants WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0}", request.VariantId)
                    .FirstOrDefaultAsync();

                if (variant == null)
                    // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                    return NotFound("Sản phẩm hoặc biến thể không tồn tại.");

                // 2. Kiểm tra tồn kho
                if (variant.AvailableStock < request.Quantity)
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest("Số lượng tồn kho không đủ để thêm vào giỏ.");

                // 3. Tìm hoặc tạo giỏ hàng cho User
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync(); // Phải lưu xuống db để có Cart.Id
                }

                // 4. Kiểm tra xem Variant này đã có trong giỏ hàng chưa
                var existingItem = cart.CartItems?.FirstOrDefault(ci => ci.VariantId == request.VariantId);

                if (existingItem != null)
                {
                    // Đã có -> Cộng dồn số lượng
                    if (existingItem.Quantity + request.Quantity > variant.AvailableStock)
                        // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
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
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    _context.CartItems.Add(newItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok("Đã thêm sản phẩm vào giỏ hàng.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                return StatusCode(500, $"Lỗi xử lý giỏ hàng: {ex.Message}");
            }
        }

        // ================= CẬP NHẬT SỐ LƯỢNG =================
        [HttpPut("{id}")]
        // [Hàm thực thi nghiệp vụ]: `UpdateQuantity` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int newQuantity)
        {
            if (newQuantity <= 0)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Số lượng phải lớn hơn 0. Nếu muốn xóa sản phẩm hãy dùng API DELETE.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(ci => ci.Id == id);

                if (cartItem == null)
                    // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                    return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

                // Kiểm tra bảo mật: Không được sửa giỏ hàng của người khác
                if (cartItem.Cart.UserId != userId)
                    // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                    return StatusCode(403, "Bạn không có quyền sửa giỏ hàng của người khác.");

                // Lock ProductVariant bằng UPDLOCK
                var variant = await _context.ProductVariants
                    .FromSqlRaw("SELECT * FROM ProductVariants WITH (UPDLOCK, HOLDLOCK) WHERE Id = {0}", cartItem.VariantId)
                    .FirstOrDefaultAsync();

                if (variant == null || variant.AvailableStock < newQuantity)
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest("Số lượng tồn kho không đủ.");

                cartItem.Quantity = newQuantity;
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;
                
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok("Cập nhật số lượng thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                return StatusCode(500, $"Lỗi cập nhật giỏ hàng: {ex.Message}");
            }
        }

        // ================= XÓA SẢN PHẨM KHỎI GIỎ HÀNG =================
        [HttpDelete("{id}")]
        // [Hàm thực thi nghiệp vụ]: `RemoveFromCart` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

            // Kiểm tra bảo mật
            if (cartItem.Cart.UserId != userId)
                // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                return StatusCode(403, "Bạn không có quyền thao tác trên giỏ hàng của người khác.");

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.CartItems.Remove(cartItem);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Đã xóa sản phẩm khỏi giỏ hàng.");
        }
    }
}
