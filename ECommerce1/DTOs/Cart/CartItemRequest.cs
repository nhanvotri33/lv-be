namespace ECommerce1.DTOs.Cart
{
    public class CartItemRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public int? AppliedCampaignId { get; set; }
        public int? ParentCartItemId { get; set; }
        public bool IsAddon { get; set; } = false;
    }
}
