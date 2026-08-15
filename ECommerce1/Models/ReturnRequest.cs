// ==========================================================================
// MODULE: ReturnRequest.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module ReturnRequest
// ==========================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public enum ReturnStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public class ReturnRequest
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User User { get; set; }

        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        public decimal TotalRefundAmount { get; set; }

        public string? AdminNote { get; set; }
        public string? GeneralNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }
}
