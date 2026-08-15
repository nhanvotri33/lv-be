// ==========================================================================
// MODULE: CategoryBrandDefaultRequest.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module CategoryBrandDefaultRequest
// ==========================================================================
using System.ComponentModel.DataAnnotations;

namespace ECommerce1.DTOs.CategoryBrandDefault
{
    public class CategoryBrandDefaultRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        public string? DefaultSpecs { get; set; } // JSON string containing key-value defaults
    }
}
