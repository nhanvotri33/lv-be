// ==========================================================================
// MODULE: InventoryTransaction.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module InventoryTransaction
// ==========================================================================
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

        // Yêu cầu đổi trả sinh ra lần nhập kho này (nếu có).
        // Trước đây việc chống nhập trùng phải dò chuỗi [ReturnReq #id] trong Note - dễ vỡ khi
        // ai đó sửa ghi chú. Có cột riêng thì đối chiếu bằng khoá, chắc chắn hơn.
        public int? ReturnRequestId { get; set; }
        // [ForeignKey("CreatedByUserId")]
        // public virtual User CreatedByUser { get; set; }
    }
}
