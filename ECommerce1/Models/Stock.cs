// ==========================================================================
// MODULE: Stock.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module Stock
// ==========================================================================
using ECommerce.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    /// <summary>
    /// 1. Ý nghĩa: Lưu từng đợt nhập hàng (giá nhập, ngày nhập, số lượng còn lại) để tính COGS (giá vốn) và hỗ trợ quản lý FIFO.
    /// 2. Luồng Thêm hàng (Nhập kho): 
    ///    - Thêm lô mới vào Stock (QuantityRemaining = QuantityIn).
    ///    - Cộng dồn số lượng vào TotalStock của ProductVariant.
    ///    - Đồng bộ tổng kho từ các Variant lên bảng Product cha.
    /// 3. Luồng Trừ hàng (Xuất kho/Bán hàng):
    ///    - Quét các lô hàng của biến thể còn hàng, sắp xếp theo ngày nhập tăng dần (FIFO - cũ nhất trừ trước).
    ///    - Trừ dần QuantityRemaining của các lô cho đến khi đủ số lượng xuất.
    ///    - Trừ số lượng tương ứng ở TotalStock của ProductVariant và đồng bộ lên Product cha.
    /// </summary>
    [Table("Stock")]
    public class Stock
    {
        [Key]
        public int StockId { get; set; }
    
        // Nối trực tiếp đến bảng ProductId (Bắt buộc) để hỗ trợ sản phẩm đơn giản (không biến thể) và tối ưu truy vấn báo cáo lô hàng không cần JOIN.
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        // Nối đến bảng ProductVariantId (Cho phép NULL) đối với sản phẩm có cấu hình màu sắc/dung lượng cụ thể.
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
