// ==========================================================================
// MODULE: ProductVariantController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module ProductVariantController
// ==========================================================================
using ECommerce1.DTOs.ProductVariant;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variantService;

        public ProductVariantController(IProductVariantService variantService)
        {
            _variantService = variantService;
        }

        // ================= READ: Lấy danh sách =================
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetAll` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAll([FromQuery] int? productId)
        {
            var variants = await _variantService.GetAllAsync(productId);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(variants);
        }

        // [API Endpoint GET [Route: `{id}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("{id}")]
        // [Hàm thực thi nghiệp vụ]: `GetById` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetById(int id)
        {
            var variant = await _variantService.GetByIdAsync(id);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(variant);
        }

        // ================= CREATE: Cần đăng nhập =================
        [HttpPost]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Create` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Create([FromBody] ProductVariantRequest request)
        {
            await _variantService.CreateAsync(request);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Tạo biến thể sản phẩm thành công.");
        }

        // ================= CREATE BATCH: Tạo nhiều biến thể cùng lúc =================
        [HttpPost("batch")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `CreateBatch` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CreateBatch([FromBody] List<ProductVariantRequest> requests)
        {
            await _variantService.CreateBatchAsync(requests);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = $"Tạo thành công {requests.Count} biến thể." });
        }

        // ================= UPDATE: Cần đăng nhập =================
        [HttpPut("{id}")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Update` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Update(int id, [FromBody] ProductVariantRequest request)
        {
            await _variantService.UpdateAsync(id, request);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cập nhật biến thể sản phẩm thành công.");
        }

        // ================= SYNC: Đồng bộ Upsert & Delete =================
        [HttpPut("sync/{productId}")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Sync` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Sync(int productId, [FromBody] List<ProductVariantRequest> requests)
        {
            await _variantService.SyncAsync(productId, requests);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Đồng bộ biến thể thành công." });
        }

        // ================= DELETE: Cần đăng nhập =================
        [HttpDelete("{id}")]
        [Authorize]
        // [Hàm thực thi nghiệp vụ]: `Delete` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Delete(int id)
        {
            await _variantService.DeleteAsync(id);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Xóa biến thể sản phẩm thành công.");
        }
    }
}
