using ECommerce.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    [Table("Stock")]
    public class Stock
    {
        [Key]
        public int StockId { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public int? VariantId { get; set; }
        [ForeignKey("VariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        public int? ReceivingDetailId { get; set; } // Liên kết với ID giao dịch nhập kho gốc (InventoryTransaction.Id)
        [ForeignKey("ReceivingDetailId")]
        public virtual InventoryTransaction? ReceivingTransaction { get; set; }

        public int QuantityIn { get; set; }
        public int QuantityRemaining { get; set; }
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    }
}
