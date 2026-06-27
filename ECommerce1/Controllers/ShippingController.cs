using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShippingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST /api/Shipping/calculate-fee
        [HttpPost("calculate-fee")]
        public async Task<IActionResult> CalculateShippingFee([FromBody] ShippingFeeRequest request)
        {
            if (string.IsNullOrEmpty(request.WardId))
            {
                return BadRequest("Vui lòng cung cấp mã phường/xã (WardId).");
            }

            // Lấy thông tin tỉnh/thành, phường/xã từ DB để tính toán phí ship động
            var ward = await _context.Wards
                .Include(w => w.Province)
                .FirstOrDefaultAsync(w => w.Id == request.WardId);

            if (ward == null)
            {
                return NotFound("Không tìm thấy khu vực được chỉ định.");
            }

            // Logic tính toán phí ship động (Ví dụ giả lập Giao Hàng Nhanh / Giao Hàng Tiết Kiệm)
            // Nếu giao nội tỉnh (ví dụ TP. Hồ Chí Minh hoặc Hà Nội) -> Phí ship rẻ (20.000đ - 25.000đ)
            // Nếu giao ngoại tỉnh -> Phí ship đắt hơn (35.000đ - 50.000đ)
            decimal baseFee = 35000;
            string provinceName = ward.Province?.Name ?? "";

            if (provinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
                provinceName.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
                provinceName.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase))
            {
                baseFee = 22000;
            }

            // Điều chỉnh phí dựa trên tổng trọng lượng/thể tích của các mặt hàng trong giỏ nếu có
            decimal weightMarkup = request.TotalWeightKg > 2 ? (request.TotalWeightKg - 2) * 5000 : 0;
            decimal finalFee = baseFee + weightMarkup;

            return Ok(new ShippingFeeResponse
            {
                Fee = finalFee,
                Carrier = "Giao Hàng Nhanh (GHN)",
                EstimatedDeliveryDays = baseFee == 22000 ? "1-2 ngày" : "3-5 ngày"
            });
        }
    }

    public class ShippingFeeRequest
    {
        public string WardId { get; set; } = string.Empty;
        public decimal TotalWeightKg { get; set; } = 1.0m;
    }

    public class ShippingFeeResponse
    {
        public decimal Fee { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string EstimatedDeliveryDays { get; set; } = string.Empty;
    }
}
