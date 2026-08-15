// ==========================================================================
// MODULE: AuditLogDtos.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module AuditLogDtos
// ==========================================================================
using System;
using System.Collections.Generic;

namespace ECommerce1.DTOs
{
    public class AuditLogFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TargetTable { get; set; } = string.Empty;
        public string? TargetId { get; set; }
        public string? TargetName { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
