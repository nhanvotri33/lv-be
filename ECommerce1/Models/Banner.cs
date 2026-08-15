// ==========================================================================
// MODULE: Banner.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module Banner
// ==========================================================================
using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string LinkUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "Slider"; // Slider, Top, Left, Right

        public bool IsActive { get; set; } = true;

        public int Position { get; set; } = 0;

        public bool IsDraft { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
