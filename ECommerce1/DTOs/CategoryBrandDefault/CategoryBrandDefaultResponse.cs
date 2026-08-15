// ==========================================================================
// MODULE: CategoryBrandDefaultResponse.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module CategoryBrandDefaultResponse
// ==========================================================================
using System;

namespace ECommerce1.DTOs.CategoryBrandDefault
{
    public class CategoryBrandDefaultResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? DefaultSpecs { get; set; } // JSON string containing key-value defaults
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
