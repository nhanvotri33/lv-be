// ==========================================================================
// MODULE: ProductController.cs
// MỤC ĐÍCH: API Controller cung cấp danh sách sản phẩm, chi tiết sản phẩm, tìm kiếm và lọc sản phẩm.
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `GetAll` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? brand = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] bool includeInactive = false)
        {
            var products = await _productService.GetAllAsync(categoryId, brand, search, sortBy, sortOrder, includeInactive);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(products);
        }

        // [API Endpoint GET [Route: `{id}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("{id:int}")]
        // [Hàm thực thi nghiệp vụ]: `GetById` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(product);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var product = await _productService.GetBySlugAsync(slug);
            return Ok(product);
        }

        // ================= CREATE: Cần đăng nhập =================
        [HttpPost]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Create` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Create([FromBody] ProductRequest request)
        {
            var productId = await _productService.CreateAsync(request);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Tạo sản phẩm thành công.", id = productId });
        }

        // ================= UPDATE: Cần đăng nhập =================
        [HttpPut("{id}")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Update` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            await _productService.UpdateAsync(id, request);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cập nhật sản phẩm thành công.");
        }

        // ================= DELETE: Cần đăng nhập =================
        [HttpDelete("{id}")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Delete` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Xóa sản phẩm thành công (Sản phẩm đã được ẩn / Xóa mềm).");
        }
    }
}
