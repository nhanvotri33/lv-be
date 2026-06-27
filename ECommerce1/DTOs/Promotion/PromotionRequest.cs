using System;

namespace ECommerce1.DTOs.Promotion
{
    public class PromotionRequest
    {
        public string Code { get; set; }
        public string DiscountType { get; set; } // "PERCENTAGE" hoặc "FIXED_AMOUNT"
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int UsageLimit { get; set; } = 0;
    }
}
