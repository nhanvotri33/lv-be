using ECommerce1.DTOs;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public interface IAuditLogService
    {
        Task<PaginatedResult<AuditLogResponseDto>> GetLogsAsync(AuditLogFilterDto filter);
    }
}
