// ==========================================================================
// MODULE: OrderItemController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module OrderItemController
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem các mặt hàng trong 1 đơn hàng cụ thể
        [HttpGet("by-order/{orderId}")]
        // [Hàm thực thi nghiệp vụ]: `GetItemsByOrder` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetItemsByOrder(int orderId)
        {
            var items = await _context.OrderItems
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductVariant.Product.Name,
                    VariantName = oi.ProductVariant.Name,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase
                })
                .ToListAsync();

            if (!items.Any())
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy dữ liệu hoặc đơn hàng không tồn tại.");

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(items);
        }
    }
}
