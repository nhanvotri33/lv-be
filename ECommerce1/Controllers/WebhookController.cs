using ECommerce.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;

        public WebhookController(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        [HttpPost("ahamove")]
        public async Task<IActionResult> AhamoveWebhook([FromBody] AhamoveWebhookRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request._id))
            {
                return BadRequest("Payload không hợp lệ.");
            }

            Console.WriteLine($"[Ahamove Webhook] Nhận cập nhật đơn hàng Ahamove ID: {request._id}, Trạng thái: {request.status}");

            // Tìm đơn hàng khớp với AhamoveOrderId
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.AhamoveOrderId == request._id);
            if (order == null)
            {
                // Trả về Ok để Ahamove không thử gửi lại (tránh spam) nếu mã đơn không khớp
                return Ok(new { message = $"Không tìm thấy đơn hàng ứng với Ahamove ID: {request._id}" });
            }

            // Cập nhật thông tin chi tiết từ webhook
            order.AhamoveStatus = request.status;
            if (!string.IsNullOrEmpty(request.shared_link))
            {
                order.AhamoveSharedLink = request.shared_link;
            }

            // Đồng bộ trạng thái đơn hàng trong hệ thống dựa trên trạng thái của Ahamove
            string upperStatus = request.status.ToUpper();
            try
            {
                if (upperStatus == "COMPLETED")
                {
                    // Map sang Đã hoàn thành (StatusId = 4)
                    await _orderService.UpdateOrderStatusAsync(order.Id, 4);
                }
                else if (upperStatus == "CANCELLED")
                {
                    // Map sang Đã hủy (StatusId = 5)
                    await _orderService.UpdateOrderStatusAsync(order.Id, 5);
                }
                else
                {
                    // Lưu lại các trạng thái trung gian (ASSIGNING, ACCEPTED, IN PROCESS...) vào DB
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ahamove Webhook Error] Cập nhật trạng thái đơn hàng {order.Id} lỗi: {ex.Message}");
                // Vẫn lưu các thay đổi nhỏ vào DB nếu có thể
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }
    }

    public class AhamoveWebhookRequest
    {
        public string _id { get; set; } = string.Empty; // Mã đơn hàng Ahamove (AhamoveOrderId)
        public string status { get; set; } = string.Empty; // Trạng thái Ahamove (e.g. COMPLETED, CANCELLED)
        public string? supplier_id { get; set; } // ID tài xế
        public string? shared_link { get; set; } // Link tracking
    }
}
