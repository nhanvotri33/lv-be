namespace ECommerce1.DTOs.Product
{
    public class ProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int TotalStock { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string ThumbnailImage { get; set; } = string.Empty;
        public string MainImage { get; set; } = string.Empty;
        public string Images { get; set; } = string.Empty;
    }
}
