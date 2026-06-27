using System;

namespace ECommerce1.DTOs.Product
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int TotalStock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string BrandName { get; set; }
        public string ThumbnailImage { get; set; }
        public string MainImage { get; set; }
        public string Images { get; set; }
        public bool IsAvailable { get; set; }
        public bool? BrandIsActive { get; set; }
    }
}
