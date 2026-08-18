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
        private readonly IAhamoveService _ahamoveService;

        public ShippingController(ApplicationDbContext context, IAhamoveService ahamoveService)
        {
            _context = context;
            _ahamoveService = ahamoveService;
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
                // Tự động đồng bộ Phường/Xã và Tỉnh/Thành từ VietnamLocationService nếu CSDL chưa lưu
                await VietnamLocationService.EnsureLocationExistsAsync(_context, request.WardId);
                dbWard = await _context.Wards
                    .Include(w => w.Province)
                    .FirstOrDefaultAsync(w => w.Id == request.WardId);
            }

            var options = new List<ShippingOption>();
            string dbProvinceName = dbWard?.Province?.Name ?? request.AddressLine ?? "";
            
            // Nếu địa chỉ thuộc TP.HCM mà chưa có tọa độ Lat/Lng từ FE gửi lên, sử dụng tọa độ mặc định trung tâm TP.HCM để ước tính phí Ahamove
            bool isHcm = dbProvinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
                         dbProvinceName.Contains("HCM", StringComparison.OrdinalIgnoreCase);

            if (isHcm)
            {
                try
                {
                    double destLat = request.Latitude.HasValue && request.Latitude.Value != 0 ? request.Latitude.Value : 10.776389;
                    double destLng = request.Longitude.HasValue && request.Longitude.Value != 0 ? request.Longitude.Value : 106.701139;

                    string wardName = dbWard?.Name ?? "";
                    string destAddress = string.IsNullOrWhiteSpace(request.AddressLine) 
                        ? $"{wardName}, {dbProvinceName}"
                        : $"{request.AddressLine}, {wardName}, {dbProvinceName}";

                    // Danh sách các dịch vụ Ahamove cung cấp tại Sài Gòn
                    var ahamoveServices = new[] 
                    {
                        new { Id = "SGN-BIKE", Name = "Ahamove (Giao Siêu Tốc)", Days = "Trong vòng 1-2 giờ" },
                        new { Id = "SGN-EXPRESS", Name = "Ahamove (Siêu Tốc - Tiết Kiệm)", Days = "Trong vòng 2-4 giờ" },
                        new { Id = "SGN-2H", Name = "Ahamove (Giao 2H - Tiết Kiệm)", Days = "Trong vòng 2 giờ" }
                    };

                    var ahamoveTasks = ahamoveServices.Select(async s => 
                    {
                        try 
                        {
                            decimal fee = await _ahamoveService.EstimateFeeAsync(destLat, destLng, destAddress, s.Id);
                            return new ShippingOption { Fee = fee, Carrier = s.Name, EstimatedDeliveryDays = s.Days };
                        }
                        catch (Exception ex)
                        { 
                            Console.WriteLine($"Lỗi khi gọi dịch vụ Ahamove ({s.Id}): {ex.Message}");
                            return null; 
                        }
                    });

                    var results = await Task.WhenAll(ahamoveTasks);
                    foreach (var res in results)
                    {
                        if (res != null) options.Add(res);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi tổng thể gọi API Ahamove: {ex.Message}. Bỏ qua tùy chọn Ahamove.");
                }
            }

            // 2. Tính phí Tiêu chuẩn (Giao Hàng Nhanh / Tiết Kiệm)
            decimal standardBaseFee = 45000;
            string estimatedDays = "3-5 ngày";
            
            if (dbProvinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
                dbProvinceName.Contains("HCM", StringComparison.OrdinalIgnoreCase))
            {
                // Nội thành TP.HCM (gần kho)
                standardBaseFee = 28000;
                estimatedDays = "1-2 ngày";
            }
            else if (dbProvinceName.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
                     dbProvinceName.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase) || 
                     dbProvinceName.Contains("Hải Phòng", StringComparison.OrdinalIgnoreCase) || 
                     dbProvinceName.Contains("Cần Thơ", StringComparison.OrdinalIgnoreCase))
            {
                // Các Thành phố lớn
                standardBaseFee = 38000;
                estimatedDays = "2-3 ngày";
            }

            decimal weightMarkup = request.TotalWeightKg > 2 ? (request.TotalWeightKg - 2) * 5000 : 0;
            decimal standardFinalFee = standardBaseFee + weightMarkup;

            options.Add(new ShippingOption
            {
                Fee = standardFinalFee,
                Carrier = "Giao Hàng Tiêu Chuẩn",
                EstimatedDeliveryDays = estimatedDays
            });

            // Nếu muốn mặc định là Giao hàng tiêu chuẩn, ta có thể đảo thứ tự hoặc Frontend sẽ tự chọn
            // Trả về kết quả đầu tiên làm thông số tương thích ngược, và toàn bộ mảng Options
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
