using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.FullName, p.CodeName })
                .ToListAsync();
            return Ok(provinces);
        }

        [HttpGet("provinces/{provinceId}/wards")]
        public async Task<IActionResult> GetWardsByProvince(string provinceId)
        {
            var wards = await _context.Wards
                .Where(w => w.ProvinceId == provinceId)
                .OrderBy(w => w.Name)
                .Select(w => new { w.Id, w.Name, w.FullName, w.CodeName, w.ProvinceId })
                .ToListAsync();
            return Ok(wards);
        }
    }
}
