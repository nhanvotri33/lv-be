using ECommerce1.DTOs.Product;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ================= READ: Ai cũng xem được =================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? brand = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] bool includeInactive = false)
        {
            var products = await _productService.GetAllAsync(categoryId, brand, search, sortBy, sortOrder, includeInactive);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }

        // ================= CREATE: Cần đăng nhập =================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductRequest request)
        {
            var productId = await _productService.CreateAsync(request);
            return Ok(new { message = "Tạo sản phẩm thành công.", id = productId });
        }

        // ================= UPDATE: Cần đăng nhập =================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            await _productService.UpdateAsync(id, request);
            return Ok("Cập nhật sản phẩm thành công.");
        }

        // ================= DELETE: Cần đăng nhập =================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok("Xóa sản phẩm thành công (Sản phẩm đã được ẩn / Xóa mềm).");
        }
    }
}
