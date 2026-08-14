using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class ReturnItem
    {
        [Key]
        public int Id { get; set; }

        public int ReturnRequestId { get; set; }
        [ForeignKey("ReturnRequestId")]
        public virtual ReturnRequest ReturnRequest { get; set; }

        public int OrderItemId { get; set; }
        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }

        public int Quantity { get; set; }

        public string Reason { get; set; }

        // Mảng URL hình ảnh minh chứng lưu dưới dạng chuỗi JSON ["url1", "url2"]
        public string? ProofImagesJson { get; set; }
    }
}
