// ==========================================================================
// MODULE: LocationController.cs
// MỤC ĐÍCH: API Controller cung cấp danh mục Tỉnh/Thành phố, Phường/Xã từ CSDL SQL Server.
// ==========================================================================
using ECommerce1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        // [API Endpoint GET [Route: `provinces`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("provinces")]
        // [Hàm thực thi nghiệp vụ]: `GetProvinces` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await VietnamLocationService.GetProvincesAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(provinces);
        }

        // [API Endpoint GET [Route: `provinces/{provinceId}/wards`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("provinces/{provinceId}/wards")]
        // [Hàm thực thi nghiệp vụ]: `GetWardsByProvince` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetWardsByProvince(string provinceId)
        {
            var wards = await VietnamLocationService.GetWardsByProvinceAsync(provinceId);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(wards);
        }
    }
}
