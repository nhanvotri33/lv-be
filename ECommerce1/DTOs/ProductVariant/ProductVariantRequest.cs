namespace ECommerce1.DTOs.ProductVariant
{
    public class ProductVariantRequest
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int TotalStock { get; set; }
        public int ProductId { get; set; }
        public string? ImageId { get; set; }
        public string? Attributes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
