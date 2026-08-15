// ==========================================================================
// MODULE: BannerDto.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module BannerDto
// ==========================================================================
namespace ECommerce1.DTOs.Banner
{
    public class BannerDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public string Type { get; set; } = "Slider"; // Slider, Top, Left, Right
        public bool IsActive { get; set; } = true;
        public int Position { get; set; } = 0;
    }
}
