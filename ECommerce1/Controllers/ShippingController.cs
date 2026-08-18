// ==========================================================================
// MODULE: ShippingController.cs
// MỤC ĐÍCH: API Controller tính phí vận chuyển động và kết nối đơn vị vận chuyển (Ahamove, GHN).
// ==========================================================================
using ECommerce.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IShippingFeeService _shippingFeeService;

        public ShippingController(ApplicationDbContext context, IShippingFeeService shippingFeeService)
        {
            _context = context;
            _shippingFeeService = shippingFeeService;
        }

        // POST /api/Shipping/calculate-fee
        [HttpPost("calculate-fee")]
        // [Hàm thực thi nghiệp vụ]: `CalculateShippingFee` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CalculateShippingFee([FromBody] ShippingFeeRequest request)
        {
            if (string.IsNullOrEmpty(request.WardId))
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Vui lòng cung cấp mã phường/xã (WardId) hoặc tọa độ.");
            }

            var dbWard = await _context.Wards
                .Include(w => w.Province)
                .FirstOrDefaultAsync(w => w.Id == request.WardId);

            if (dbWard == null)
            {
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy khu vực được chỉ định.");
            }

            // Toàn bộ bảng giá nằm ở IShippingFeeService để checkout thu đúng số đã báo ở đây.
            var quotes = await _shippingFeeService.GetQuotesAsync(
                dbWard.Province?.Name ?? "",
                dbWard.Name ?? "",
                request.AddressLine,
                request.Latitude,
                request.Longitude,
                request.TotalWeightKg);

            var options = quotes.Select(q => new ShippingOption
            {
                Fee = q.Fee,
                Carrier = q.Carrier,
                EstimatedDeliveryDays = q.EstimatedDeliveryDays
            }).ToList();

            return Ok(new ShippingFeeResponse
            {
                Fee = options[0].Fee,
                Carrier = options[0].Carrier,
                EstimatedDeliveryDays = options[0].EstimatedDeliveryDays,
                Options = options
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

    public class ShippingOption
    {
        public decimal Fee { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string EstimatedDeliveryDays { get; set; } = string.Empty;
    }

    public class ShippingFeeResponse
    {
        public decimal Fee { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string EstimatedDeliveryDays { get; set; } = string.Empty;
        public List<ShippingOption> Options { get; set; } = new List<ShippingOption>();
    }
}
