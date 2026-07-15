using ECommerce.Models;
using ECommerce1.Services;
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
        private readonly IAhamoveService _ahamoveService;

        public ShippingController(ApplicationDbContext context, IAhamoveService ahamoveService)
        {
            _context = context;
            _ahamoveService = ahamoveService;
        }

        // POST /api/Shipping/calculate-fee
        [HttpPost("calculate-fee")]
        public async Task<IActionResult> CalculateShippingFee([FromBody] ShippingFeeRequest request)
        {
            // Nếu có tọa độ thì ưu tiên tính phí ship bằng Ahamove
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                try
                {
                    var ward = await _context.Wards
                        .Include(w => w.Province)
                        .FirstOrDefaultAsync(w => w.Id == request.WardId);

                    string provinceName = ward?.Province?.Name ?? "";
                    string wardName = ward?.Name ?? "";
                    string destAddress = $"{request.AddressLine}, {wardName}, {provinceName}";

                    decimal ahamoveFee = await _ahamoveService.EstimateFeeAsync(request.Latitude.Value, request.Longitude.Value, destAddress);
                    return Ok(new ShippingFeeResponse
                    {
                        Fee = ahamoveFee,
                        Carrier = "Ahamove (Siêu tốc)",
                        EstimatedDeliveryDays = "2-4 giờ"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi gọi API Ahamove trong ShippingController: {ex.Message}. Sử dụng cách tính phí mặc định làm phương án dự phòng.");
                }
            }

            if (string.IsNullOrEmpty(request.WardId))
            {
                return BadRequest("Vui lòng cung cấp mã phường/xã (WardId) hoặc tọa độ.");
            }

            // Lấy thông tin tỉnh/thành, phường/xã từ DB để tính toán phí ship động
            var dbWard = await _context.Wards
                .Include(w => w.Province)
                .FirstOrDefaultAsync(w => w.Id == request.WardId);

            if (dbWard == null)
            {
                return NotFound("Không tìm thấy khu vực được chỉ định.");
            }

            // Logic tính toán phí ship động (Ví dụ giả lập Giao Hàng Nhanh / Giao Hàng Tiết Kiệm)
            decimal baseFee = 35000;
            string dbProvinceName = dbWard.Province?.Name ?? "";

            if (dbProvinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
                dbProvinceName.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
                dbProvinceName.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase))
            {
                baseFee = 22000;
            }

            // Điều chỉnh phí dựa trên tổng trọng lượng/thể tích của các mặt hàng trong giỏ nếu có
            decimal weightMarkup = request.TotalWeightKg > 2 ? (request.TotalWeightKg - 2) * 5000 : 0;
            decimal finalFee = baseFee + weightMarkup;

            return Ok(new ShippingFeeResponse
            {
                Fee = finalFee,
                Carrier = "Giao hàng tiêu chuẩn",
                EstimatedDeliveryDays = baseFee == 22000 ? "1-2 ngày" : "3-5 ngày"
            });
        }
    }

    public class ShippingFeeRequest
    {
        public string WardId { get; set; } = string.Empty;
        public decimal TotalWeightKg { get; set; } = 1.0m;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressLine { get; set; } = string.Empty;
    }

    public class ShippingFeeResponse
    {
        public decimal Fee { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string EstimatedDeliveryDays { get; set; } = string.Empty;
    }
}
