using ECommerce1.DTOs;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<AuditLogResponseDto>> GetLogsAsync(AuditLogFilterDto filter)
        {
            var query = _context.AuditLogs.AsQueryable();

            // 1. Lọc theo từ khóa (Email hoặc UserId)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchStr = filter.Search.Trim().ToLower();
                query = query.Where(l => 
                    (l.UserEmail != null && l.UserEmail.ToLower().Contains(searchStr)) || 
                    (l.UserId != null && l.UserId.ToLower() == searchStr)
                );
            }

            // 2. Lọc theo loại hành động (Create, Update, Delete)
            if (!string.IsNullOrWhiteSpace(filter.ActionType))
            {
                query = query.Where(l => l.Action == filter.ActionType);
            }

            // 3. Lọc theo khoảng thời gian (Sử dụng cột Timestamp)
            if (filter.StartDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                // Lấy hết ngày cuối cùng (đến 23:59:59)
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(l => l.Timestamp <= endOfDay);
            }

            // 4. Tính toán phân trang
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            // 5. Thực hiện phân trang và ánh xạ sang DTO
            var logs = await query
                .OrderByDescending(l => l.Timestamp) // Sắp xếp theo Timestamp mới nhất
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new AuditLogResponseDto
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    UserId = l.UserId, 
                    UserEmail = l.UserEmail,
                    Action = l.Action,
                    TargetTable = l.TargetTable,
                    TargetId = l.TargetId, 
                    OldValues = l.OldValues,
                    NewValues = l.NewValues
                })
                .ToListAsync();

            return new PaginatedResult<AuditLogResponseDto>
            {
                Items = logs,
                TotalCount = totalCount,
                TotalPages = totalPages > 0 ? totalPages : 1
            };
        }
    }
}
