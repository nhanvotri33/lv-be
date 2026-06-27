using System;

namespace ECommerce1.DTOs.InventoryTransaction
{
    public class InventoryTransactionRequest
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; } // If null, we'll try to find a default variant
        public int QuantityChanged { get; set; }
        public string TransactionType { get; set; } // "IMPORT_SUPPLIER", "IMPORT_RETURN", "EXPORT_SELL", "EXPORT_DEFECT"
        public decimal Price { get; set; }
        public string Note { get; set; }
    }
}
