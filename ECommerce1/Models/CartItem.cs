using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerce.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }

        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        public int VariantId { get; set; }
        [ForeignKey("VariantId")]
        public virtual ProductVariant ProductVariant { get; set; }

        public int? AppliedCampaignId { get; set; }
        [ForeignKey("AppliedCampaignId")]
        public virtual PromotionCampaign? AppliedCampaign { get; set; }

        // TỰ LIÊN KẾT: Lưu ID dòng giỏ hàng của sản phẩm chính để bảo hộ ưu đãi cho sản phẩm phụ mua kèm
        public int? ParentCartItemId { get; set; }
        [ForeignKey("ParentCartItemId")]
        public virtual CartItem? ParentCartItem { get; set; }

        public bool IsAddon { get; set; } = false;
    }
}
