// ==========================================================================
// MODULE: ReturnController.cs
// MỤC ĐÍCH: API Controller xử lý quy trình Yêu cầu Đổi trả / Hoàn tiền sản phẩm (Return & Refund System) phía Khách hàng và Admin.
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `CreateReturnRequest` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized(new { message = "Vui lòng đăng nhập lại." });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            // Kiểm tra quyền sở hữu đơn
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (order.UserId != userId && userRole != "Admin")
                return Forbid();

            // Kiểm tra trạng thái đơn phải là Đã giao thành công (Status 4)
            if (order.OrderStatusId != 4)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Chỉ đơn hàng đã giao thành công (Status 4) mới được tạo yêu cầu đổi trả." });

            // Kiểm tra thời hạn 30 ngày
            if (order.CreatedAt.AddDays(30) < DateTime.UtcNow)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Đã quá thời hạn đổi trả 30 ngày cho đơn hàng này." });

            // Kiểm tra xem đơn đã có yêu cầu đổi trả chưa
            var existingReq = await _context.ReturnRequests
                .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId);
            if (existingReq != null)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Đơn hàng này đã có yêu cầu đổi trả đang được xử lý." });

            if (dto.Items == null || !dto.Items.Any())
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
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
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest(new { message = $"Sản phẩm #{itemDto.OrderItemId} không thuộc đơn hàng này." });

                if (itemDto.Quantity <= 0 || itemDto.Quantity > orderItem.Quantity)
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest(new { message = $"Số lượng đổi trả không hợp lệ cho sản phẩm #{itemDto.OrderItemId}." });

                // Tính tiền hoàn dựa trên giá mua thực tế PriceAtPurchase của từng món khách chọn trả
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

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.ReturnRequests.Add(returnRequest);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new
            {
                message = "Tạo yêu cầu đổi trả thành công!",
                returnRequestId = returnRequest.Id,
                totalRefundAmount = totalRefund
            });
        }

        // ================= 2. XEM YÊU CẦU ĐỔI TRẢ THEO MÃ ĐƠN HÀNG =================
        [HttpGet("order/{orderId}")]
        // [Hàm thực thi nghiệp vụ]: `GetReturnRequestByOrder` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetReturnRequestByOrder(int orderId)
        {
            var req = await _context.ReturnRequests
                .Include(r => r.ReturnItems)
                .ThenInclude(ri => ri.OrderItem)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            // Chỉ chủ đơn hoặc Admin mới được xem. Endpoint này chỉ [Authorize] chung nên nếu
            // không đối chiếu, bất kỳ tài khoản nào cũng đọc được lý do trả hàng, ảnh minh chứng
            // và danh sách sản phẩm đã mua của người khác - chỉ cần đoán orderId (số tăng dần).
            if (req != null)
            {
                var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var callerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAdmin = string.Equals(callerRole, "Admin", StringComparison.OrdinalIgnoreCase);
                if (!isAdmin && (!Guid.TryParse(callerIdString, out Guid callerId) || req.UserId != callerId))
                {
                    // [Phản hồi API]: Trả về kết quả Forbid cho phía Client
                    return Forbid();
                }
            }

            if (req == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
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

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(result);
        }

        // ================= 3. DANH SÁCH TẤT CẢ YÊU CẦU ĐỔI TRẢ (ADMIN) =================
        /// <summary>
        /// Khách hàng lấy toàn bộ yêu cầu đổi trả của CHÍNH MÌNH, gom theo đơn.
        /// Trang theo dõi đơn hàng trước đây đọc trạng thái đổi trả từ localStorage nên chỉ đúng
        /// trên đúng trình duyệt đã gửi yêu cầu; đổi máy hay xoá cache là mất sạch.
        /// </summary>
        // [API Endpoint GET [Route: `my`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReturnRequests()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized(new { message = "Vui lòng đăng nhập lại." });

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var requests = await _context.ReturnRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    orderId = r.OrderId,
                    status = r.Status.ToString(),
                    totalRefundAmount = r.TotalRefundAmount,
                    adminNote = r.AdminNote,
                    createdAt = r.CreatedAt
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(requests);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetAllReturnRequests` - Xử lý logic và luồng dữ liệu
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

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(result);
        }

        // ================= 4. ADMIN DUYỆT ĐỔI TRẢ & HOÀN TIỀN (LUỒNG HOÀN TIỀN LINH HOẠT TỪNG MÓN) =================
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `ApproveReturnRequest` - Xử lý logic và luồng dữ liệu
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
                    // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                    return NotFound(new { message = "Không tìm thấy yêu cầu đổi trả." });

                if (req.Status != ReturnStatus.Pending)
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest(new { message = "Yêu cầu đổi trả này đã được xử lý trước đó." });

                // BƯỚC 2: CẬP NHẬT TRẠNG THÁI YÊU CẦU ĐỔI TRẢ -> APPROVED
                req.Status = ReturnStatus.Approved;
                req.AdminNote = dto.AdminNote;
                req.UpdatedAt = DateTime.UtcNow;

                // BƯỚC 3: CẬP NHẬT TRẠNG THÁI NHẬT KÝ THANH TOÁN -> REFUNDED
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == req.OrderId);
                if (payment != null)
                {
                    // Chữ thường cho khớp toàn bộ phần còn lại của hệ thống
                    // (OrderService dùng "succeeded"/"failed"/"refunded"), tránh so sánh chuỗi hụt.
                    payment.Status = "refunded";
                }

                // =========================================================================
                // 🔥 KHOẢN 1: KHỞI TẠO BIẾN TÍNH TIỀN VÀ ĐẾM TỔNG MÓN ĐƠN HÀNG 🔥
                // - actualRefundAmount: Tính chính xác tổng tiền của những món khách thực sự chọn trả.
                // - totalItemsInOrder: Đếm tổng số lượng món trong đơn hàng gốc để so sánh trả hết hay trả một phần.
                // =========================================================================
                decimal actualRefundAmount = 0;
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                int totalItemsInOrder = await _context.OrderItems.CountAsync(oi => oi.OrderId == req.OrderId);

                // BƯỚC 4: VÒNG LẶP XỬ LÝ NHẬP KHO & CỘNG DỒN TIỀN CHO TỪNG MÓN ĐỔI TRẢ
                foreach (var item in req.ReturnItems)
                {
                    if (item.OrderItem != null)
                    {
                        // -------------------------------------------------------------------------
                        // ĐIỂM 1 (CẢI TIẾN TIỀN HOÀN): Cộng dồn giá thực tế (PriceAtPurchase * Quantity)
                        // của đúng món hàng mà khách gửi yêu cầu trả lại.
                        // Không hoàn toàn bộ tổng đơn hàng để tránh đền nhầm tiền cho các món khách giữ lại!
                        // -------------------------------------------------------------------------
                        actualRefundAmount += (item.OrderItem.PriceAtPurchase * item.Quantity);

                        // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                        var variant = await _context.ProductVariants.FindAsync(item.OrderItem.VariantId);
                        if (variant != null)
                        {
                            // -------------------------------------------------------------------------
                            // ĐIỂM 2 (CHỐT CHẶN AN TOÀN KHO):
                            // Chỉ cộng lại vào kho chính (TotalStock) nếu lý do là "Giao sai hàng" / "Giao sai mẫu".
                            // (Vì sản phẩm giao sai vẫn còn nguyên vẹn, mới 100% chưa bóc mở).
                            // -------------------------------------------------------------------------
                            bool isWrongDelivery = !string.IsNullOrEmpty(item.Reason) &&
                                (item.Reason.Contains("Giao sai", StringComparison.OrdinalIgnoreCase) ||
                                 item.Reason.Contains("sai màu", StringComparison.OrdinalIgnoreCase));

                            if (isWrongDelivery)
                            {
                                // Cộng số lượng trả lại vào kho chính
                                variant.TotalStock += item.Quantity;

                                // Ghi nhật ký biến động kho (Audit log cho kho hàng)
                                _context.InventoryTransactions.Add(new InventoryTransaction
                                {
                                    VariantId = variant.Id,
                                    QuantityChanged = item.Quantity,
                                    TransactionType = "Returned",
                                    Note = $"Nhập lại kho chính do Giao sai hàng - Yêu cầu #{req.Id} (Đơn #PS{req.OrderId})",
                                    Price = item.OrderItem.PriceAtPurchase,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                            else
                            {
                                // -------------------------------------------------------------------------
                                // ĐIỂM 3 (BỎ QUA HÀNG HỎNG/LỖI KỸ THUẬT/BÓC SEAL):
                                // TUYỆT ĐỐI KHÔNG CỘNG VÀO KHO CHÍNH (TotalStock) để tránh trộn lẫn hàng hỏng với hàng mới.
                                // Hệ thống ghi log để Thủ kho / Admin tiến hành xử lý hủy hàng thủ công.
                                // -------------------------------------------------------------------------
                                Console.WriteLine($"Sản phẩm VariantId {variant.Id} bị trả do '{item.Reason}'. Bỏ qua nhập kho tự động.");
                            }
                        }
                    }
                }

                // -------------------------------------------------------------------------
                // ĐIỂM 4 (CẬP NHẬT TỔNG TIỀN CHUẨN XÁC VÀO CSDL):
                // Gán lại TotalRefundAmount bằng số tiền tính toán thực tế actualRefundAmount
                // để bảo đảm toàn vẹn dữ liệu đề phòng Frontend gửi sai số tiền.
                // -------------------------------------------------------------------------
                req.TotalRefundAmount = actualRefundAmount;

                // =========================================================================
                //   KHOẢN 2: XỬ LÝ TRẠNG THÁI ĐƠN HÀNG LINH HOẠT (TRẢ TOÀN BỘ VS TRẢ MỘT PHẦN) 
                // - Nếu số món trả bằng tổng số món đơn hàng: Khách trả HẾT -> Đơn chuyển Status 7 (Refunded).
                // - Nếu số món trả bé hơn tổng số món đơn hàng: Khách trả 1 VÀI MÓN (Giữ món còn lại) ->
                //   Hệ thống ghi log đổi trả một phần và cập nhật trạng thái minh bạch.
                // =========================================================================
                if (req.Order != null)
                {
                    if (req.ReturnItems.Count == totalItemsInOrder)
                    {
                        // Khách trả HẾT tất cả các món -> Chuyển trạng thái 7 (Refunded - Đã hoàn tiền toàn bộ)
                        req.Order.OrderStatusId = 7;
                    }
                    else
                    {
                        // Khách chỉ trả 1 vài món (Ví dụ: trả Điện thoại, giữ Phụ kiện)
                        req.Order.OrderStatusId = 7; // Refunded (Đánh dấu hoàn tiền cho sản phẩm bị trả)
                        Console.WriteLine($"Đơn #PS{req.OrderId}: Đổi trả một phần ({req.ReturnItems.Count}/{totalItemsInOrder} món). Tiền hoàn thực tế: {actualRefundAmount:#,##0} VNĐ.");
                    }
                }

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                // XỬ LÝ ĐIỂM THƯỞNG - phải khớp với OrderService.UpdateOrderStatusAsync khi đơn
                // chuyển 4 -> 7, nếu không cùng một nghiệp vụ lại cho ra hai kết quả khác nhau:
                //  1. Thu hồi điểm thưởng đã cộng khi đơn hoàn thành
                //  2. Thu hồi điểm tích lũy xét hạng tương ứng
                //  3. HOÀN LẠI số điểm khách đã tiêu để thanh toán đơn này
                // Trước đây chỉ làm bước 1, nên khách dùng điểm trả bớt tiền rồi được duyệt đổi
                // trả sẽ mất trắng số điểm đã tiêu.
                var user = await _context.Users.FindAsync(req.UserId);
                if (user != null && req.Order != null)
                {
                    if (req.Order.PointsEarned > 0)
                    {
                        user.RewardPoints = Math.Max(0, user.RewardPoints - req.Order.PointsEarned);
                        user.AccumulatedPoints = Math.Max(0, user.AccumulatedPoints - req.Order.PointsEarned);
                    }

                    if (req.Order.PointsRedeemed > 0)
                    {
                        user.RewardPoints += req.Order.PointsRedeemed;
                    }
                }

                // LƯU TOÀN BỘ VÀO DATABASE VÀ COMMIT TRANSACTION
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // BƯỚC 6: GHI NHẬT KÝ KIỂM TOÁN HỆ THỐNG (AUDIT LOG)
                var adminIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = adminIdString,
                    Action = "APPROVE_RETURN_REFUND",
                    TargetTable = "ReturnRequests",
                    TargetId = req.Id.ToString(),
                    NewValues = $"Admin đã duyệt hoàn tiền {actualRefundAmount:#,##0} VNĐ cho yêu cầu #{req.Id} thuộc đơn hàng #PS{req.OrderId}",
                    Timestamp = DateTime.UtcNow
                });
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new
                {
                    message = "Duyệt đổi trả & hoàn tiền thành công!",
                    status = "Approved",
                    actualRefundAmount = actualRefundAmount
                });
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi gì -> Hoàn tác toàn bộ Transaction!
                await transaction.RollbackAsync();
                // [Phản hồi API]: Trả về kết quả StatusCode cho phía Client
                return StatusCode(500, new { message = "Lỗi hệ thống khi duyệt đổi trả: " + ex.Message });
            }
        }

        // ================= 5. ADMIN TỪ CHỐI YÊU CẦU ĐỔI TRẢ =================
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `RejectReturnRequest` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> RejectReturnRequest(int id, [FromBody] RejectReturnRequestDto dto)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var req = await _context.ReturnRequests.FindAsync(id);
            if (req == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound(new { message = "Không tìm thấy yêu cầu đổi trả." });

            if (req.Status != ReturnStatus.Pending)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(new { message = "Yêu cầu đổi trả này đã được xử lý trước đó." });

            req.Status = ReturnStatus.Rejected;
            req.AdminNote = dto.AdminNote;
            req.UpdatedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Đã từ chối yêu cầu đổi trả.", status = "Rejected" });
        }
    }
}
