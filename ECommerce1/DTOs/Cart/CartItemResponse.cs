namespace ECommerce1.DTOs.Cart
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
    }
}
