// ==========================================================================
// MODULE: CartController.cs
// MỤC ĐÍCH: API Controller quản lý giỏ hàng người dùng (Thêm, Sửa, Xóa sản phẩm trong giỏ).
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
        // [Hàm thực thi nghiệp vụ]: `GetMyCart` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetMyCart()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Không thể xác định người dùng.");
            }

            // Tìm giỏ hàng của User
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Warranty)
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
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Carts.Add(cart);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
            }

            // Lấy danh sách item
            var items = cart.CartItems?.ToList() ?? new System.Collections.Generic.List<CartItem>();
            var responseItems = new System.Collections.Generic.List<CartItemResponse>();

            bool dbChanged = false;

            foreach (var item in items)
            {
                decimal price = item.ProductVariant?.Price ?? 0;

                // Tính giá Campaign discount nếu đây là phụ kiện mua kèm
                if (item.AppliedCampaignId.HasValue && item.IsAddon)
                {
                    var campaign = await _context.PromotionCampaigns
                        .Include(c => c.MainProductRules)
                        .FirstOrDefaultAsync(c => c.Id == item.AppliedCampaignId.Value && c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow);

                    if (campaign != null)
                    {
                        CartItem? parentItem = null;
                        if (item.ParentCartItemId.HasValue)
                        {
                            parentItem = items.FirstOrDefault(ci => ci.Id == item.ParentCartItemId && !ci.IsAddon);
                        }
                        
                        if (parentItem == null)
                        {
                            parentItem = items.FirstOrDefault(ci => 
                                !ci.IsAddon && 
                                (campaign.MainProductRules == null || !campaign.MainProductRules.Any() || 
                                 campaign.MainProductRules.Any(r => 
                                    (r.ProductId.HasValue && r.ProductId == ci.ProductVariant?.ProductId) ||
                                    (r.CategoryId.HasValue && r.CategoryId == ci.ProductVariant?.Product?.CategoryId) ||
                                    (r.BrandId.HasValue && r.BrandId == ci.ProductVariant?.Product?.BrandId)
                                 ))
                            );
                        }

                        if (parentItem != null)
                        {
                            if (item.ParentCartItemId != parentItem.Id)
                            {
                                item.ParentCartItemId = parentItem.Id;
                                dbChanged = true;
                            }

                            if (campaign.DiscountType == "Percentage")
                                price = price * (1 - campaign.DiscountValue / 100);
                            else if (campaign.DiscountType == "FixedAmount")
                                price = Math.Max(0, price - campaign.DiscountValue);
                            else if (campaign.DiscountType == "FixedPrice")
                                price = campaign.DiscountValue;
                        }
                        else
                        {
                            item.AppliedCampaignId = null;
                            item.ParentCartItemId = null;
                            item.IsAddon = false;
                            dbChanged = true;
                        }
                    }
                }

                responseItems.Add(new CartItemResponse
                {
                    Id = item.Id,
                    VariantId = item.VariantId,
                    ProductId = item.ProductVariant?.ProductId ?? 0,
                    ProductName = item.ProductVariant?.Product?.Name ?? "Sản phẩm đã xóa",
                    VariantName = item.ProductVariant?.Name ?? "Biến thể đã xóa",
                    ImageUrl = item.ProductVariant?.ImageId ?? item.ProductVariant?.Product?.ThumbnailImage,
                    Price = price,
                    Quantity = item.Quantity,
                    AppliedCampaignId = item.AppliedCampaignId,
                    ParentCartItemId = item.ParentCartItemId,
                    IsAddon = item.IsAddon,
                    WarrantyId = item.WarrantyId,
                    WarrantyName = item.Warranty?.Name,
                    WarrantyPrice = item.Warranty?.BasePrice ?? 0
                });
            }

            if (dbChanged)
            {
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
            }

            // Map ra DTO
            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = responseItems
            };

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }

        // Làm sạch toàn bộ giỏ hàng
        [HttpDelete("clear")]
        // [Hàm thực thi nghiệp vụ]: `ClearCart` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ClearCart()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems != null && cart.CartItems.Any())
            {
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
            }

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Đã làm sạch giỏ hàng.");
        }

        // Đồng bộ giỏ hàng hàng loạt (Batch Sync)
        [HttpPost("sync")]
        // [Hàm thực thi nghiệp vụ]: `SyncCart` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> SyncCart([FromBody] System.Collections.Generic.List<SyncCartRequest> items)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
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
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Carts.Add(cart);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
            }

            // Xóa sạch các item cũ
            if (cart.CartItems != null && cart.CartItems.Any())
            {
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.CartItems.RemoveRange(cart.CartItems);
            }

            // Thêm các item mới. Lưu tạm cặp (CartItem đang chờ persist, request gốc) để pass 2
            // gắn ParentCartItemId cho addon theo ParentProductId của SP chính.
            var pendingPairs = new System.Collections.Generic.List<(CartItem CartItem, SyncCartRequest Req, int ProductId)>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    // Kiểm tra xem variant có tồn tại không
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                    if (variant == null) continue;

                    // RÀNG BUỘC TỒN KHO KHẢ DỤNG: Tối đa bằng TotalStock - ReservedStock
                    int availStock = Math.Max(0, variant.TotalStock - variant.ReservedStock);
                    if (availStock <= 0) continue; // Hết hàng thì bỏ qua

                    int safeQuantity = Math.Min(item.Quantity, availStock);

                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        VariantId = item.VariantId,
                        Quantity = safeQuantity,
                        AppliedCampaignId = item.AppliedCampaignId,
                        ParentCartItemId = null, // Sẽ resolve ở pass 2 dưới đây
                        IsAddon = item.IsAddon,
                        WarrantyId = item.WarrantyId
                    };
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    _context.CartItems.Add(cartItem);
                    pendingPairs.Add((cartItem, item, variant.ProductId));
                }
            }

            cart.UpdatedAt = DateTime.UtcNow;
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // Pass 2: Sau khi có Id của mọi CartItem, gắn ParentCartItemId cho addon
            // dựa trên ParentProductId FE gửi lên. Giữ combo discount ổn định qua checkout.
            bool anyLinked = false;
            foreach (var (cartItem, req, _) in pendingPairs)
            {
                if (!req.IsAddon || !req.ParentProductId.HasValue) continue;
                var parent = pendingPairs
                    .Where(p => !p.Req.IsAddon && p.ProductId == req.ParentProductId.Value)
                    .Select(p => p.CartItem)
                    .FirstOrDefault();
                if (parent != null && parent.Id != cartItem.Id)
                {
                    cartItem.ParentCartItemId = parent.Id;
                    anyLinked = true;
                }
            }
            if (anyLinked)
            {
                await _context.SaveChangesAsync();
            }

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Đồng bộ giỏ hàng thành công.");
        }
    }

    public class SyncCartRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public int? AppliedCampaignId { get; set; }
        public int? ParentCartItemId { get; set; }
        public bool IsAddon { get; set; } = false;
        public int? WarrantyId { get; set; }
        // ProductId của SP chính do FE gửi kèm để gắn Parent chính xác cho addon
        public int? ParentProductId { get; set; }
    }
}
