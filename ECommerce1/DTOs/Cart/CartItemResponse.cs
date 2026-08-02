namespace ECommerce1.DTOs.Cart
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
        public int? AppliedCampaignId { get; set; }
        public int? ParentCartItemId { get; set; }
        public bool IsAddon { get; set; }

        // Trường thông tin bổ sung cho bảo hành đi kèm
        public int? WarrantyId { get; set; }
        public string? WarrantyName { get; set; }
        public decimal WarrantyPrice { get; set; } = 0;
    }
}
