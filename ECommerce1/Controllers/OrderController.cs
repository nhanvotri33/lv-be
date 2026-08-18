// ==========================================================================
// MODULE: OrderController.cs
// MỤC ĐÍCH: API Controller tiếp nhận và điều hướng các yêu cầu mua hàng, đặt hàng (Checkout), tra cứu và quản lý đơn hàng.
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `GetMyOrders` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            var orders = await _orderService.GetMyOrdersAsync(userId);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(orders);
        }

        // ================= XEM TẤT CẢ ĐƠN HÀNG (ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetAllOrders` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(orders);
        }

        // ================= ĐẶT HÀNG (CHECKOUT) TỪ GIỎ HÀNG =================
        [HttpPost("checkout")]
        // [Hàm thực thi nghiệp vụ]: `Checkout` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            try
            {
                var result = await _orderService.CheckoutAsync(userId, request);
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================= HỦY ĐƠN HÀNG (DÀNH CHO KHÁCH HÀNG) =================
        [HttpPut("{id}/cancel")]
        [AllowAnonymous]
        // [Hàm thực thi nghiệp vụ]: `CancelOrder` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CancelOrder(int id, [FromQuery] string? phoneNumber = null)
        {
            Guid? userId = null;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid parsedId))
            {
                userId = parsedId;
            }

            await _orderService.CancelOrderAsync(id, userId, phoneNumber);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Hủy đơn hàng thành công.");
        }

        // ================= CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (ADMIN) =================
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `UpdateOrderStatus` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] int newStatusId)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatusId);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cập nhật trạng thái đơn hàng và xử lý tồn kho thành công.");
        }

        // ================= GỬI GIAO HÀNG QUA AHAMOVE (ADMIN) =================
        [HttpPost("{id}/ship-ahamove")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `ShipWithAhamove` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ShipWithAhamove(int id)
        {
            try
            {
                var response = await _orderService.ShipWithAhamoveAsync(id);
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(response);
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(ex.Message);
            }
        }

        // ================= TRA CỨU ĐƠN HÀNG (DÀNH CHO KHÁCH VÃNG LAI) =================
        [HttpGet("track")]
        [AllowAnonymous]
        // [Hàm thực thi nghiệp vụ]: `TrackOrder` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> TrackOrder([FromQuery] string orderId, [FromQuery] string phoneNumber)
        {
            if (string.IsNullOrEmpty(orderId))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã đơn hàng không hợp lệ.");

            if (!int.TryParse(orderId, out int id))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã đơn hàng không hợp lệ.");

            var response = await _orderService.TrackOrderAsync(id, phoneNumber);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }
    }
}
