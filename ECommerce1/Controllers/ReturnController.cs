using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReturnController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReturnController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= 1. KHÁCH HÀNG TẠO YÊU CẦU ĐỔI TRẢ =================
        [HttpPost]
        public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized(new { message = "Vui lòng đăng nhập lại." });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            // Kiểm tra quyền sở hữu đơn
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (order.UserId != userId && userRole != "Admin")
                return Forbid();

            // Kiểm tra trạng thái đơn phải là Đã giao thành công (Status 4)
            if (order.OrderStatusId != 4)
                return BadRequest(new { message = "Chỉ đơn hàng đã giao thành công (Status 4) mới được tạo yêu cầu đổi trả." });

            // Kiểm tra thời hạn 7 ngày
            if (order.CreatedAt.AddDays(7) < DateTime.UtcNow)
                return BadRequest(new { message = "Đã quá thời hạn đổi trả 7 ngày cho đơn hàng này." });

            // Kiểm tra xem đơn đã có yêu cầu đổi trả chưa
            var existingReq = await _context.ReturnRequests
                .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId);
            if (existingReq != null)
                return BadRequest(new { message = "Đơn hàng này đã có yêu cầu đổi trả đang được xử lý." });

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { message = "Vui lòng chọn ít nhất 1 sản phẩm cần đổi trả." });

            var returnRequest = new ReturnRequest
            {
                OrderId = dto.OrderId,
                UserId = order.UserId,
                Status = ReturnStatus.Pending,
                GeneralNote = dto.GeneralNote,
                CreatedAt = DateTime.UtcNow,
                TotalRefundAmount = 0
            };

            decimal totalRefund = 0;

            foreach (var itemDto in dto.Items)
            {
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.OrderItemId);
                if (orderItem == null)
                    return BadRequest(new { message = $"Sản phẩm #{itemDto.OrderItemId} không thuộc đơn hàng này." });

                if (itemDto.Quantity <= 0 || itemDto.Quantity > orderItem.Quantity)
                    return BadRequest(new { message = $"Số lượng đổi trả không hợp lệ cho sản phẩm #{itemDto.OrderItemId}." });

                decimal itemRefund = itemDto.Quantity * orderItem.PriceAtPurchase;
                totalRefund += itemRefund;

                string proofImagesJson = itemDto.ProofImages != null && itemDto.ProofImages.Any()
                    ? JsonSerializer.Serialize(itemDto.ProofImages)
                    : "[]";

                returnRequest.ReturnItems.Add(new ReturnItem
                {
                    OrderItemId = itemDto.OrderItemId,
                    Quantity = itemDto.Quantity,
                    Reason = itemDto.Reason ?? "Không có lý do",
                    ProofImagesJson = proofImagesJson
                });
            }

            returnRequest.TotalRefundAmount = totalRefund;

            _context.ReturnRequests.Add(returnRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo yêu cầu đổi trả thành công!",
                returnRequestId = returnRequest.Id,
                totalRefundAmount = totalRefund
            });
        }

        // ================= 2. XEM YÊU CẦU ĐỔI TRẢ THEO MÃ ĐƠN HÀNG =================
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetReturnRequestByOrder(int orderId)
        {
            var req = await _context.ReturnRequests
                .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.OrderItem)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            if (req == null)
                return NotFound(new { message = "Chưa có yêu cầu đổi trả cho đơn hàng này." });

            var result = new
            {
                id = req.Id,
                orderId = req.OrderId,
                userId = req.UserId,
                status = req.Status.ToString(),
                totalRefundAmount = req.TotalRefundAmount,
                adminNote = req.AdminNote,
                generalNote = req.GeneralNote,
                createdAt = req.CreatedAt,
                returnItems = req.ReturnItems.Select(ri => new
                {
                    id = ri.Id,
                    orderItemId = ri.OrderItemId,
                    productName = ri.OrderItem?.ProductVariant?.Product?.Name ?? "Sản phẩm",
                    quantity = ri.Quantity,
                    reason = ri.Reason,
                    proofImages = !string.IsNullOrEmpty(ri.ProofImagesJson)
                        ? JsonSerializer.Deserialize<List<string>>(ri.ProofImagesJson)
                        : new List<string>()
                })
            };

            return Ok(result);
        }

        // ================= 3. DANH SÁCH TẤT CẢ YÊU CẦU ĐỔI TRẢ (ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReturnRequests()
        {
            var requests = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.OrderItem)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = requests.Select(req => new
            {
                id = req.Id,
                orderId = req.OrderId,
                userId = req.UserId,
                customerName = req.User?.Username,
                customerEmail = req.User?.Email,
                status = req.Status.ToString(),
                totalRefundAmount = req.TotalRefundAmount,
                adminNote = req.AdminNote,
                createdAt = req.CreatedAt,
                itemsCount = req.ReturnItems.Count
            });

            return Ok(result);
        }

        // ================= 4. ADMIN DUYỆT ĐỔI TRẢ & HOÀN TIỀN (LUỒNG 7 BƯỚC AN TOÀN KHO) =================
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveReturnRequest(int id, [FromBody] ApproveReturnRequestDto dto)
        {
            // BẮT ĐẦU DATABASE TRANSACTION: Đảm bảo đồng bộ dữ liệu Tiền - Kho - Đơn hàng (ACID)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // BƯỚC 1: LẤY THÔNG TIN & VALIDATE YÊU CẦU ĐỔI TRẢ
                var req = await _context.ReturnRequests
                    .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.OrderItem)
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (req == null)
                    return NotFound(new { message = "Không tìm thấy yêu cầu đổi trả." });

                if (req.Status != ReturnStatus.Pending)
                    return BadRequest(new { message = "Yêu cầu đổi trả này đã được xử lý trước đó." });

                // BƯỚC 2: CẬP NHẬT TRẠNG THÁI YÊU CẦU ĐỔI TRẢ -> APPROVED
                req.Status = ReturnStatus.Approved;
                req.AdminNote = dto.AdminNote;
                req.UpdatedAt = DateTime.UtcNow;

                // BƯỚC 3: CẬP NHẬT TRẠNG THÁI NHẬT KÝ THANH TOÁN -> REFUNDED
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == req.OrderId);
                if (payment != null)
                {
                    payment.Status = "Refunded";
                }

                // BƯỚC 4: XỬ LÝ NHẬP KHO VỚI CHỐT CHẶN AN TOÀN KHO (RESTOCK SAFETY CHECK)
                foreach (var item in req.ReturnItems)
                {
                    if (item.OrderItem != null)
                    {
                        var variant = await _context.ProductVariants.FindAsync(item.OrderItem.VariantId);
                        if (variant != null)
                        {
                            // =========================================================================
                            // Chỉ cộng lại vào kho chính nếu lý do chính xác là "Giao sai hàng" / "Giao sai mẫu"
                            // (Hàng giao sai vẫn còn nguyên seal, mới 100% chưa bóc mở).
                            // =========================================================================
                            bool isWrongDelivery = !string.IsNullOrEmpty(item.Reason) &&
                                (item.Reason.Contains("Giao sai", StringComparison.OrdinalIgnoreCase) ||
                                 item.Reason.Contains("sai màu", StringComparison.OrdinalIgnoreCase));

                            if (isWrongDelivery)
                            {
                                // Cộng lại số lượng vào tồn kho chính
                                variant.TotalStock += item.Quantity;

                                // Ghi nhật ký biến động kho (Audit log cho kho hàng)
                                _context.InventoryTransactions.Add(new InventoryTransaction
                                {
                                    VariantId = variant.Id,
                                    QuantityChanged = item.Quantity,
                                    TransactionType = "Returned", // Hàng trả lại
                                    Note = $"Nhập lại kho do Giao sai hàng - Yêu cầu #{req.Id} (Đơn #PS{req.OrderId})",
                                    Price = item.OrderItem.PriceAtPurchase,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                            else
                            {
                                // =========================================================================
                                // LOGIC CHO CÁC LÝ DO CÒN LẠI (Lỗi kỹ thuật, hỏng hóc vận chuyển, bóc seal...):
                                // KHÔNG LÀM GÌ CẢ VỚI BẢNG KHO CHÍNH (TotalStock)!
                                // Tránh nhầm lẫn nhập hàng lỗi/vỡ vào kho hàng mới kinh doanh.
                                // Chỉ ghi log lại để Thủ kho biết đường kiểm tra và xử lý thủ công.
                                // Hết thời gian triển khai rồi, mệt quá
                                // Nhiểu bảng trong SQL quá rồi
                                // =========================================================================
                                Console.WriteLine($"Sản phẩm VariantId {variant.Id} bị trả do '{item.Reason}'. Bỏ qua nhập kho tự động.");
                            }
                        }
                    }
                }

                // BƯỚC 5: CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (OrderStatusId = 7 - Refunded) & THU HỒI ĐIỂM REWARD
                req.Order.OrderStatusId = 7; // Status 7 = Refunded

                var user = await _context.Users.FindAsync(req.UserId);
                if (user != null && req.Order.PointsEarned > 0)
                {
                    user.RewardPoints = Math.Max(0, user.RewardPoints - req.Order.PointsEarned);
                }

                // LƯU TOÀN BỘ VÀO DATABASE VÀ COMMIT TRANSACTION
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // BƯỚC 6: GHI NHẬT KÝ KIỂM TOÁN HỆ THỐNG (AUDIT LOG)
                var adminIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = adminIdString,
                    Action = "APPROVE_RETURN_REFUND",
                    TargetTable = "ReturnRequests",
                    TargetId = req.Id.ToString(),
                    NewValues = $"Admin đã duyệt hoàn tiền cho yêu cầu #{req.Id} thuộc đơn hàng #PS{req.OrderId}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Ok(new { message = "Duyệt đổi trả & hoàn tiền thành công!", status = "Approved" });
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi gì (Lỗi code, lỗi CSDL, lỗi API thanh toán) => Hoàn tác toàn bộ!
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống khi duyệt đổi trả: " + ex.Message });
            }
        }

        // ================= 5. ADMIN TỪ CHỐI YÊU CẦU ĐỔI TRẢ =================
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectReturnRequest(int id, [FromBody] RejectReturnRequestDto dto)
        {
            var req = await _context.ReturnRequests.FindAsync(id);
            if (req == null)
                return NotFound(new { message = "Không tìm thấy yêu cầu đổi trả." });

            if (req.Status != ReturnStatus.Pending)
                return BadRequest(new { message = "Yêu cầu đổi trả này đã được xử lý trước đó." });

            req.Status = ReturnStatus.Rejected;
            req.AdminNote = dto.AdminNote;
            req.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối yêu cầu đổi trả.", status = "Rejected" });
        }
    }
}
