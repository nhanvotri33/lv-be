using ECommerce1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await VietnamLocationService.GetProvincesAsync();
            return Ok(provinces);
        }

        [HttpGet("provinces/{provinceId}/wards")]
        public async Task<IActionResult> GetWardsByProvince(string provinceId)
        {
            var wards = await VietnamLocationService.GetWardsByProvinceAsync(provinceId);
            return Ok(wards);
        }
    }
}
