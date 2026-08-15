// ==========================================================================
// MODULE: BlogResponse.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module BlogResponse
// ==========================================================================
using System;

namespace ECommerce1.DTOs.Blog
{
    public class BlogResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
        public int ViewCount { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UserId { get; set; }
        public string? AuthorName { get; set; }
    }
}
