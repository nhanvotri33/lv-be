using System;

namespace ECommerce1.DTOs.InventoryTransaction
{
    public class InventoryTransactionResponse
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; }
        public int QuantityChanged { get; set; }
        public string TransactionType { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public string CreatedByUsername { get; set; }
        public bool IsReverted { get; set; }
    }
}

