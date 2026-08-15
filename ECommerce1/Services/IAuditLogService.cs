// ==========================================================================
// MODULE: IAuditLogService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module IAuditLogService
// ==========================================================================
using ECommerce1.DTOs;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IAuditLogService
    {
        Task<PaginatedResult<AuditLogResponseDto>> GetLogsAsync(AuditLogFilterDto filter);
    }
}
