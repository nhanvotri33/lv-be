using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        public int VariantId { get; set; }
        [ForeignKey("VariantId")]
        public virtual ProductVariant ProductVariant { get; set; }

        public int QuantityChanged { get; set; } // Positive for import/return, negative for sale/damage

        public string TransactionType { get; set; } // E.g., "Import", "Sale", "Return", "Damage"

        public string Note { get; set; } // E.g., "Sold for Order #102" or "Supplier restock"

        public decimal Price { get; set; }

        public bool IsReverted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedByUserId { get; set; }
        // [ForeignKey("CreatedByUserId")]
        // public virtual User CreatedByUser { get; set; }
    }
}
