using System;

namespace ECommerce1.DTOs.Promotion
{
    public class PromotionResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
    }
}
