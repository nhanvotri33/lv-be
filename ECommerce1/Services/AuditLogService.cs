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

            // Populate TargetName (Tên thực tế của bản ghi: Username/Email/Product Name...)
            var userGuids = logs.Where(l => l.TargetTable == "Users" && !string.IsNullOrEmpty(l.TargetId) && Guid.TryParse(l.TargetId, out _))
                                .Select(l => Guid.Parse(l.TargetId!)).Distinct().ToList();
            var userMap = await _context.Users.Where(u => userGuids.Contains(u.Id))
                                .ToDictionaryAsync(u => u.Id.ToString(), u => u.Username ?? u.Email);

            var productIds = logs.Where(l => l.TargetTable == "Products" && !string.IsNullOrEmpty(l.TargetId) && int.TryParse(l.TargetId, out _))
                                .Select(l => int.Parse(l.TargetId!)).Distinct().ToList();
            var productMap = await _context.Products.Where(p => productIds.Contains(p.Id))
                                .ToDictionaryAsync(p => p.Id.ToString(), p => p.Name);

            var warrantyIds = logs.Where(l => l.TargetTable == "Warranties" && !string.IsNullOrEmpty(l.TargetId) && int.TryParse(l.TargetId, out _))
                                .Select(l => int.Parse(l.TargetId!)).Distinct().ToList();
            var warrantyMap = await _context.Warranties.Where(w => warrantyIds.Contains(w.Id))
                                .ToDictionaryAsync(w => w.Id.ToString(), w => w.Name);

            var brandIds = logs.Where(l => l.TargetTable == "Brands" && !string.IsNullOrEmpty(l.TargetId) && int.TryParse(l.TargetId, out _))
                                .Select(l => int.Parse(l.TargetId!)).Distinct().ToList();
            var brandMap = await _context.Brands.Where(b => brandIds.Contains(b.Id))
                                .ToDictionaryAsync(b => b.Id.ToString(), b => b.Name);

            var catIds = logs.Where(l => l.TargetTable == "Categories" && !string.IsNullOrEmpty(l.TargetId) && int.TryParse(l.TargetId, out _))
                                .Select(l => int.Parse(l.TargetId!)).Distinct().ToList();
            var catMap = await _context.Categories.Where(c => catIds.Contains(c.Id))
                                .ToDictionaryAsync(c => c.Id.ToString(), c => c.Name);

            foreach (var item in logs)
            {
                if (item.TargetTable == "Users" && item.TargetId != null && userMap.TryGetValue(item.TargetId, out var uName))
                    item.TargetName = uName;
                else if (item.TargetTable == "Products" && item.TargetId != null && productMap.TryGetValue(item.TargetId, out var pName))
                    item.TargetName = pName;
                else if (item.TargetTable == "Warranties" && item.TargetId != null && warrantyMap.TryGetValue(item.TargetId, out var wName))
                    item.TargetName = wName;
                else if (item.TargetTable == "Brands" && item.TargetId != null && brandMap.TryGetValue(item.TargetId, out var bName))
                    item.TargetName = bName;
                else if (item.TargetTable == "Categories" && item.TargetId != null && catMap.TryGetValue(item.TargetId, out var cName))
                    item.TargetName = cName;
            }

            return new PaginatedResult<AuditLogResponseDto>
            {
                Items = logs,
                TotalCount = totalCount,
                TotalPages = totalPages > 0 ? totalPages : 1
            };
        }
    }
}
