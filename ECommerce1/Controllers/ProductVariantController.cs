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
        public async Task<IActionResult> GetAll([FromQuery] int? productId)
        {
            var variants = await _variantService.GetAllAsync(productId);
            return Ok(variants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var variant = await _variantService.GetByIdAsync(id);
            return Ok(variant);
        }

        // ================= CREATE: Cần đăng nhập =================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductVariantRequest request)
        {
            await _variantService.CreateAsync(request);
            return Ok("Tạo biến thể sản phẩm thành công.");
        }

        // ================= CREATE BATCH: Tạo nhiều biến thể cùng lúc =================
        [HttpPost("batch")]
        [Authorize]
        public async Task<IActionResult> CreateBatch([FromBody] List<ProductVariantRequest> requests)
        {
            await _variantService.CreateBatchAsync(requests);
            return Ok(new { message = $"Tạo thành công {requests.Count} biến thể." });
        }

        // ================= UPDATE: Cần đăng nhập =================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductVariantRequest request)
        {
            await _variantService.UpdateAsync(id, request);
            return Ok("Cập nhật biến thể sản phẩm thành công.");
        }

        // ================= SYNC: Đồng bộ Upsert & Delete =================
        [HttpPut("sync/{productId}")]
        [Authorize]
        public async Task<IActionResult> Sync(int productId, [FromBody] List<ProductVariantRequest> requests)
        {
            await _variantService.SyncAsync(productId, requests);
            return Ok(new { message = "Đồng bộ biến thể thành công." });
        }

        // ================= DELETE: Cần đăng nhập =================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _variantService.DeleteAsync(id);
            return Ok("Xóa biến thể sản phẩm thành công.");
        }
    }
}
