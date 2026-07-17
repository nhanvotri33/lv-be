using System;

namespace ECommerce1.DTOs.InventoryTransaction
{
    public class InventoryDetailDTO
    {
        public int InventoryDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ReceivingDetailId { get; set; } // Nullable vì adjustment không có receiving
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public string? TransactionCode { get; set; }
        public int QuantityIn { get; set; }
        public int QuantityRemaining { get; set; }
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}
