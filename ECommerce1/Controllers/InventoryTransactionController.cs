// ==========================================================================
// MODULE: InventoryTransactionController.cs
// MỤC ĐÍCH: API Controller quản lý nhập - xuất - hoàn tác kho hàng và truy vết lịch sử giao dịch kho.
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `GetAll` - Xử lý logic và luồng dữ liệu
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

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }

        // ================= POST: Thực hiện giao dịch kho (ADMIN) =================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Create` - Xử lý logic và luồng dữ liệu
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
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
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
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest($"Tồn kho hiện tại của '{variant.Name}' không đủ ({variant.TotalStock} sản phẩm).");
            }

            // Cập nhật tồn kho (Kiểm tra tình trạng máy đối với nhập hàng khách trả)
            bool shouldUpdateStock = true;
            if (type == "IMPORT_RETURN" && !string.IsNullOrEmpty(request.Note))
            {
                if (request.Note.Contains("[Tình trạng: Đã bóc seal / Máy cũ]") || 
                    request.Note.Contains("[Tình trạng: Lỗi phần cứng]"))
                {
                    shouldUpdateStock = false;
                }
            }

            if (shouldUpdateStock)
            {
                // Cập nhật tồn kho ở biến thể
                variant.TotalStock += actualQtyChange;
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

            // TỰ ĐỘNG ĐỒNG BỘ TRẠNG THÁI ĐƠN HÀNG SANG "ĐỔI TRẢ / HOÀN TIỀN" (StatusId = 7)
            if (type == "IMPORT_RETURN")
            {
                int targetOrderId = request.OrderId ?? 0;
                if (targetOrderId <= 0 && !string.IsNullOrEmpty(request.Note))
                {
                    // Thử trích xuất OrderId từ Note dạng "[Đơn hàng #ORD123]" hoặc "#123"
                    var match = System.Text.RegularExpressions.Regex.Match(request.Note, @"#?(?:ORD)?(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedId))
                    {
                        targetOrderId = extractedId;
                    }
                }

                if (targetOrderId > 0)
                {
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    var orderToUpdate = await _context.Orders.FindAsync(targetOrderId);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.OrderStatusId = 7; // 7 = Refunded (Đổi trả / Hoàn tiền)
                        // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                        _context.Orders.Update(orderToUpdate);
                    }
                }
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.InventoryTransactions.Add(transaction);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // Xử lý lưu lô hàng vào Stock
            if (actualQtyChange > 0)
            {
                // THÊM HÀNG: Tạo mới lô hàng nhập (QuantityRemaining = Số lượng nhập mới)
                var newDetail = new Stock
                {
                    ProductId = variant.ProductId,
                    VariantId = variant.Id,
                    QuantityIn = actualQtyChange,
                    QuantityRemaining = actualQtyChange,
                    Unit = "Cái",
                    Price = request.Price,
                    ReceivedDate = DateTime.UtcNow,
                    ReceivingDetailId = transaction.Id
                };
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Stocks.Add(newDetail);
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
            }
            else if (actualQtyChange < 0)
            {
                // TRỪ HÀNG (FIFO): Lấy các lô hàng sắp xếp theo ngày nhập cũ nhất để trừ dần số lượng còn lại của từng lô (QuantityRemaining)
                int qtyToDeduct = Math.Abs(actualQtyChange);
                var availableLots = await _context.Stocks
                    .Where(d => d.ProductId == variant.ProductId && d.QuantityRemaining > 0)
                    .OrderBy(d => d.ReceivedDate)
                    .ToListAsync();

                 foreach (var lot in availableLots)
                 {
                     if (qtyToDeduct <= 0) break;
                     if (lot.QuantityRemaining >= qtyToDeduct)
                     {
                         lot.QuantityRemaining -= qtyToDeduct;
                         qtyToDeduct = 0;
                     }
                     else
                     {
                         qtyToDeduct -= lot.QuantityRemaining;
                         lot.QuantityRemaining = 0;
                     }
                 }
                 // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                 await _context.SaveChangesAsync();
            }

            // Cập nhật tồn kho tổng ở Product atomically từ tổng các biến thể để tránh race condition
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET TotalStock = COALESCE((SELECT SUM(TotalStock) FROM ProductVariants WHERE ProductId = {0}), 0), " +
                "ReservedStock = COALESCE((SELECT SUM(ReservedStock) FROM ProductVariants WHERE ProductId = {0}), 0) WHERE Id = {0}; " +
                "UPDATE Products SET IsActive = CAST(0 AS BIT) WHERE Id = {0} AND (TotalStock - ReservedStock) <= 0;",
                variant.ProductId);

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { Message = "Thực hiện giao dịch kho thành công.", TransactionId = transaction.Id, NewStock = variant.TotalStock });
        }

        // ================= PUT: Hoàn tác giao dịch kho (ADMIN) =================
        [HttpPut("{id}/revert")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `Revert` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Revert(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Include(t => t.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy giao dịch.");
            }

            if (transaction.IsReverted)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Giao dịch này đã được hoàn tác trước đó.");
            }

            var variant = transaction.ProductVariant;
            if (variant == null)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Không tìm thấy thông tin sản phẩm liên kết với giao dịch.");
            }

            // Đảo ngược số lượng thay đổi
            int qtyToRevert = -transaction.QuantityChanged;

            // Nếu đảo ngược dẫn đến tồn kho âm, cảnh báo
            if (qtyToRevert < 0 && (variant.TotalStock + qtyToRevert) < 0)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest($"Không thể hoàn tác. Số lượng tồn kho sau hoàn tác của '{variant.Name}' sẽ bị âm.");
            }

            // Cập nhật tồn kho ở biến thể
            variant.TotalStock += qtyToRevert;

            // Xử lý hoàn tác trong bảng Stock
            if (transaction.QuantityChanged > 0)
            {
                var stockItem = await _context.Stocks
                    .FirstOrDefaultAsync(d => d.ReceivingDetailId == transaction.Id);
                if (stockItem != null)
                {
                    stockItem.QuantityRemaining = 0;
                }
            }
            else if (transaction.QuantityChanged < 0)
            {
                var newDetail = new Stock
                {
                    ProductId = variant.ProductId,
                    VariantId = variant.Id,
                    QuantityIn = Math.Abs(transaction.QuantityChanged),
                    QuantityRemaining = Math.Abs(transaction.QuantityChanged),
                    Unit = "Cái",
                    Price = transaction.Price,
                    ReceivedDate = DateTime.UtcNow,
                    ReceivingDetailId = transaction.Id
                };
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.Stocks.Add(newDetail);
            }

            transaction.IsReverted = true;
            transaction.Note += " (Đã hoàn tác)";

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // Cập nhật tồn kho tổng ở Product atomically từ tổng các biến thể để tránh race condition
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET TotalStock = COALESCE((SELECT SUM(TotalStock) FROM ProductVariants WHERE ProductId = {0}), 0), " +
                "ReservedStock = COALESCE((SELECT SUM(ReservedStock) FROM ProductVariants WHERE ProductId = {0}), 0) WHERE Id = {0}; " +
                "UPDATE Products SET IsActive = CAST(0 AS BIT) WHERE Id = {0} AND (TotalStock - ReservedStock) <= 0;",
                variant.ProductId);

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { Message = "Hoàn tác giao dịch kho thành công.", NewStock = variant.TotalStock });
        }

        // ================= GET: Xem tồn kho chi tiết (ADMIN) =================
        [HttpGet("stock")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetStockDetails` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetStockDetails()
        {
            var stocks = await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.ProductVariant)
                .Include(s => s.ReceivingTransaction)
                .OrderByDescending(s => s.ReceivedDate)
                .ToListAsync();

            var response = stocks.Select(s => {
                // =========================================================================
                // [ĐỊNH DẠNG MÃ GIAO DỊCH LÔ HÀNG - PHÍA BACK-END]
                // - Mục đích: Sinh mã giao dịch phục vụ hiển thị chi tiết lô hàng tồn kho.
                // - Nếu Giảng viên yêu cầu đổi cấu trúc mã (Ví dụ: PS thành PSXB):
                //   👉 Hãy sửa các giá trị chuỗi gán cho biến 'prefix' bên dưới (Ví dụ: prefix = "PSXB").
                //   👉 Lưu ý: Sửa đồng bộ với file FE: components/HistoryTable.jsx dòng 128-142.
                // =========================================================================
                string transactionCode = "Điều chỉnh";
                if (s.ReceivingDetailId.HasValue)
                {
                    string prefix = "TX"; // Tiền tố mặc định
                    if (s.ReceivingTransaction != null)
                    {
                        if (s.ReceivingTransaction.TransactionType == "IMPORT_SUPPLIER")
                            prefix = "ORD";  // Nhập từ nhà cung cấp
                        else if (s.ReceivingTransaction.TransactionType == "IMPORT_RETURN")
                            prefix = "REO";  // Nhập trả hàng lỗi từ khách
                        else if (s.ReceivingTransaction.TransactionType == "EXPORT_SELL")
                            prefix = "PS";   // Xuất bán hàng
                        else if (s.ReceivingTransaction.TransactionType == "EXPORT_DEFECT")
                            prefix = "ER";   // Xuất trả hàng lỗi cho nhà cung cấp
                    }
                    transactionCode = $"#{prefix}{s.ReceivingDetailId}"; // Ghép thành mã chứng từ đầy đủ
                }

                return new InventoryDetailDTO
                {
                    InventoryDetailId = s.StockId,
                    ProductId = s.ProductId,
                    ProductName = s.Product?.Name ?? "Sản phẩm không xác định",
                    ReceivingDetailId = s.ReceivingDetailId,
                    VariantId = s.VariantId,
                    VariantName = s.ProductVariant?.Name,
                    TransactionCode = transactionCode,
                    QuantityIn = s.QuantityIn,
                    QuantityRemaining = s.QuantityRemaining,
                    Unit = s.Unit ?? "Cái",
                    Price = s.Price,
                    ReceivedDate = s.ReceivedDate
                };
            }).ToList();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }
    }
}
