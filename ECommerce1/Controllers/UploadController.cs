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
        public async Task<IActionResult> UploadLocal(IFormFile file)
        {
            try
            {
                var url = await _fileService.UploadImageAsync(file);
                return Ok(new { Url = url, Message = "Upload local thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

