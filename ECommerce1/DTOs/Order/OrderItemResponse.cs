// ==========================================================================
// MODULE: OrderItemResponse.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module OrderItemResponse
// ==========================================================================
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

        // Các thuộc tính bảo hành đi kèm và thẩm định
        public int? WarrantyId { get; set; }
        public string? WarrantyName { get; set; }
        public decimal WarrantyPrice { get; set; }
        public int? CustomerDeviceId { get; set; }
        public string? ImeiOrSerial { get; set; }
        public string? CustomerDeviceProductName { get; set; }
        public string? InspectionStatus { get; set; }
    }
}
