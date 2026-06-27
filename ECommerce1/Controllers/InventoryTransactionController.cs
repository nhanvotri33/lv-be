using ECommerce.Models;
using ECommerce1.DTOs.InventoryTransaction;
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
    public class InventoryTransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= GET: Lấy lịch sử giao dịch kho (ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.InventoryTransactions
                .Include(t => t.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Lấy danh sách users để map username/email
            var users = await _context.Users.ToDictionaryAsync(u => u.Id, u => u.Username);

            var response = transactions.Select(t => new InventoryTransactionResponse
            {
                Id = t.Id,
                OrderId = null,
                ProductId = t.ProductVariant?.ProductId ?? 0,
                ProductName = t.ProductVariant?.Product?.Name ?? "Sản phẩm không xác định",
                VariantId = t.VariantId,
                VariantName = t.ProductVariant?.Name ?? "Mặc định",
                QuantityChanged = t.QuantityChanged,
                TransactionType = t.TransactionType,
                Price = t.Price,
                Note = t.Note,
                CreatedAt = t.CreatedAt,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUsername = t.CreatedByUserId.HasValue && users.ContainsKey(t.CreatedByUserId.Value) 
                    ? users[t.CreatedByUserId.Value] 
                    : "Hệ thống",
                IsReverted = t.IsReverted
            }).ToList();

            // Lấy danh sách các đơn hàng đã thanh toán hoặc đã giao/hoàn trả để hiển thị trong lịch sử xuất/nhập kho
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Where(o => o.OrderStatusId == 2 || o.OrderStatusId == 3 || o.OrderStatusId == 4 || o.OrderStatusId == 7)
                .ToListAsync();

            foreach (var order in orders)
            {
                if (order.OrderItems == null) continue;
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant == null) continue;

                    int qtyChanged = (order.OrderStatusId == 7) ? item.Quantity : -item.Quantity;
                    string txType = (order.OrderStatusId == 7) ? "IMPORT_RETURN" : "EXPORT_SELL";
                    string note = (order.OrderStatusId == 7) 
                        ? $"Khách trả hàng (Đơn hàng #{order.Id})" 
                        : $"Bán hàng cho khách (Đơn hàng #{order.Id})";

                    response.Add(new InventoryTransactionResponse
                    {
                        Id = 100000 + item.Id,
                        OrderId = order.Id,
                        ProductId = item.ProductVariant.ProductId,
                        ProductName = item.ProductVariant.Product?.Name ?? "Sản phẩm không xác định",
                        VariantId = item.VariantId,
                        VariantName = item.ProductVariant.Name ?? "Mặc định",
                        QuantityChanged = qtyChanged,
                        TransactionType = txType,
                        Price = item.PriceAtPurchase,
                        Note = note,
                        CreatedAt = order.CreatedAt,
                        CreatedByUserId = order.UserId,
                        CreatedByUsername = order.ReceiverName ?? "Khách hàng",
                        IsReverted = false
                    });
                }
            }

            response = response.OrderByDescending(t => t.CreatedAt).ToList();

            return Ok(response);
        }

        // ================= POST: Thực hiện giao dịch kho (ADMIN) =================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] InventoryTransactionRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? createdByUserId = null;
            if (Guid.TryParse(userIdString, out Guid parsedId))
            {
                createdByUserId = parsedId;
            }

            // Tìm ProductVariant
            ProductVariant variant = null;
            if (request.VariantId.HasValue && request.VariantId.Value > 0)
            {
                variant = await _context.ProductVariants
                    .Include(pv => pv.Product)
                    .FirstOrDefaultAsync(pv => pv.Id == request.VariantId.Value);
            }
            else
            {
                // Tìm biến thể đầu tiên của Product gốc
                variant = await _context.ProductVariants
                    .Include(pv => pv.Product)
                    .FirstOrDefaultAsync(pv => pv.ProductId == request.ProductId);
            }

            if (variant == null)
            {
                return BadRequest("Không tìm thấy biến thể hoặc sản phẩm hợp lệ.");
            }

            // Xác định dấu số lượng dựa trên loại giao dịch
            int actualQtyChange = Math.Abs(request.QuantityChanged);
            string type = request.TransactionType.ToUpper();
            if (type == "EXPORT_SELL" || type == "EXPORT_DEFECT")
            {
                actualQtyChange = -actualQtyChange;
            }

            // Kiểm tra tồn kho nếu xuất kho
            if (actualQtyChange < 0 && (variant.TotalStock + actualQtyChange) < 0)
            {
                return BadRequest($"Tồn kho hiện tại của '{variant.Name}' không đủ ({variant.TotalStock} sản phẩm).");
            }

            // Cập nhật tồn kho ở biến thể
            variant.TotalStock += actualQtyChange;

            // Cập nhật tồn kho tổng ở Product
            if (variant.Product != null)
            {
                variant.Product.TotalStock += actualQtyChange;
                if (variant.Product.TotalStock < 0) variant.Product.TotalStock = 0;
            }

            // Tạo bản ghi giao dịch
            var transaction = new InventoryTransaction
            {
                VariantId = variant.Id,
                QuantityChanged = actualQtyChange,
                TransactionType = request.TransactionType,
                Price = request.Price,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId,
                IsReverted = false
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Thực hiện giao dịch kho thành công.", TransactionId = transaction.Id, NewStock = variant.TotalStock });
        }

        // ================= PUT: Hoàn tác giao dịch kho (ADMIN) =================
        [HttpPut("{id}/revert")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Revert(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Include(t => t.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound("Không tìm thấy giao dịch.");
            }

            if (transaction.IsReverted)
            {
                return BadRequest("Giao dịch này đã được hoàn tác trước đó.");
            }

            var variant = transaction.ProductVariant;
            if (variant == null)
            {
                return BadRequest("Không tìm thấy thông tin sản phẩm liên kết với giao dịch.");
            }

            // Đảo ngược số lượng thay đổi
            int qtyToRevert = -transaction.QuantityChanged;

            // Nếu đảo ngược dẫn đến tồn kho âm, cảnh báo
            if (qtyToRevert < 0 && (variant.TotalStock + qtyToRevert) < 0)
            {
                return BadRequest($"Không thể hoàn tác. Số lượng tồn kho sau hoàn tác của '{variant.Name}' sẽ bị âm.");
            }

            // Cập nhật tồn kho
            variant.TotalStock += qtyToRevert;
            if (variant.Product != null)
            {
                variant.Product.TotalStock += qtyToRevert;
                if (variant.Product.TotalStock < 0) variant.Product.TotalStock = 0;
            }

            transaction.IsReverted = true;
            transaction.Note += " (Đã hoàn tác)";

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Hoàn tác giao dịch kho thành công.", NewStock = variant.TotalStock });
        }
    }
}
