// ==========================================================================
// MODULE: AuditLogController.cs
// MỤC ĐÍCH: API Controller phía Admin xem nhật ký hệ thống (Audit Logs) để kiểm toán an toàn.
// ==========================================================================
using ECommerce1.DTOs;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // [API Endpoint GET]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetLogs` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
        {
            var result = await _auditLogService.GetLogsAsync(filter);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(result);
        }
    }
}
