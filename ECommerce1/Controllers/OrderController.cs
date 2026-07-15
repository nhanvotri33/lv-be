using ECommerce1.DTOs.Order;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ================= XEM DANH SÁCH ĐƠN HÀNG CỦA TÔI =================
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        // ================= XEM TẤT CẢ ĐƠN HÀNG (ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // ================= ĐẶT HÀNG (CHECKOUT) TỪ GIỎ HÀNG =================
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var result = await _orderService.CheckoutAsync(userId, request);
            return Ok(result);
        }

        // ================= HỦY ĐƠN HÀNG (DÀNH CHO KHÁCH HÀNG) =================
        [HttpPut("{id}/cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelOrder(int id, [FromQuery] string? phoneNumber = null)
        {
            Guid? userId = null;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid parsedId))
            {
                userId = parsedId;
            }

            await _orderService.CancelOrderAsync(id, userId, phoneNumber);
            return Ok("Hủy đơn hàng thành công.");
        }

        // ================= CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (ADMIN) =================
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] int newStatusId)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatusId);
            return Ok("Cập nhật trạng thái đơn hàng và xử lý tồn kho thành công.");
        }

        // ================= GỬI GIAO HÀNG QUA AHAMOVE (ADMIN) =================
        [HttpPost("{id}/ship-ahamove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ShipWithAhamove(int id)
        {
            try
            {
                var response = await _orderService.ShipWithAhamoveAsync(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================= TRA CỨU ĐƠN HÀNG (DÀNH CHO KHÁCH VÃNG LAI) =================
        [HttpGet("track")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackOrder([FromQuery] string orderId, [FromQuery] string phoneNumber)
        {
            if (string.IsNullOrEmpty(orderId))
                return BadRequest("Mã đơn hàng không hợp lệ.");

            if (!int.TryParse(orderId, out int id))
                return BadRequest("Mã đơn hàng không hợp lệ.");

            var response = await _orderService.TrackOrderAsync(id, phoneNumber);
            return Ok(response);
        }
    }
}
