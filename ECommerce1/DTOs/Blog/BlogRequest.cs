using System.ComponentModel.DataAnnotations;

namespace ECommerce1.DTOs.Blog
{
    public class BlogRequest
    {
        [Required(ErrorMessage = "Tiêu đề bài viết không được để trống")]
        [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug không được để trống")]
        [MaxLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự")]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
        public string Content { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [MaxLength(100)]
        public string? Author { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(255)]
        public string? Tags { get; set; }

        public bool IsPublished { get; set; } = true;

        public bool IsFeatured { get; set; } = false;
    }
}
