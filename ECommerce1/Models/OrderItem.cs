using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerce.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public int VariantId { get; set; }
        [ForeignKey("VariantId")]
        public virtual ProductVariant ProductVariant { get; set; }

        public int? AppliedCampaignId { get; set; }
        [ForeignKey("AppliedCampaignId")]
        public virtual PromotionCampaign? AppliedCampaign { get; set; }

        public decimal CampaignDiscountAmount { get; set; } = 0;

        public int? ParentOrderItemId { get; set; }
        [ForeignKey("ParentOrderItemId")]
        public virtual OrderItem? ParentOrderItem { get; set; }

        public bool IsAddon { get; set; } = false;
    }
}
