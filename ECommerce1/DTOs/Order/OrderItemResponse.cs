namespace ECommerce1.DTOs.Order
{
    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal SubTotal => Quantity * PriceAtPurchase;
    }
}
