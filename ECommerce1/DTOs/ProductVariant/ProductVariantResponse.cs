using System;

namespace ECommerce1.DTOs.ProductVariant
{
    public class ProductVariantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int TotalStock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProductId { get; set; }
        public string ImageId { get; set; }
        public string Attributes { get; set; }
        public bool IsActive { get; set; }
    }
}
