using System.ComponentModel.DataAnnotations;

namespace ECommerce1.DTOs.Brand
{
    public class BrandRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;
        
        public string BrandCode { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}
