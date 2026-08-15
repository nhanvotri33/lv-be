// ==========================================================================
// MODULE: Blog.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module Blog
// ==========================================================================
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [MaxLength(100)]
        public string? Author { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(255)]
        public string? Tags { get; set; }

        public int ViewCount { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User? User { get; set; }
    }
}
