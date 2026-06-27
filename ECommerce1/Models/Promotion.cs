using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerce.Models
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public int UsageLimit { get; set; } // Giới hạn số lượng mã (0 là không giới hạn)
        public int UsedCount { get; set; }  // Số lượng mã đã được sử dụng

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PromotionUsage> PromotionUsages { get; set; }
    }
}
