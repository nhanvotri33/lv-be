// ==========================================================================
// MODULE: UploadController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module UploadController
// ==========================================================================
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")] // Bật dòng này nếu chỉ muốn Admin được upload ảnh
    public class UploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public UploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // ================= UPLOAD LOCAL =================
        [HttpPost("local")]
        // [Hàm thực thi nghiệp vụ]: `UploadLocal` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UploadLocal(IFormFile file, [FromQuery] string folder = "general")
        {
            try
            {
                var url = await _fileService.UploadImageAsync(file, folder);
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new { Url = url, Message = "Upload local thành công!" });
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(ex.Message);
            }
        }
    }
}

