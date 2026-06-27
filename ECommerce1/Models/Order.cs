using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerce.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User User { get; set; }

        public int OrderStatusId { get; set; }
        [ForeignKey("OrderStatusId")]
        public virtual OrderStatus OrderStatus { get; set; }

        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ShippingAddressLine { get; set; }
        public string ShippingWard { get; set; }
        public string ShippingProvince { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "COD";

        public int? PromotionId { get; set; }
        [ForeignKey("PromotionId")]
        public virtual Promotion? Promotion { get; set; }

        public int PointsEarned { get; set; } = 0;
        public int PointsRedeemed { get; set; } = 0;
        public decimal DiscountFromPoints { get; set; } = 0;
        
        public string? Note { get; set; } // Ghi chú giao hàng từ khách hàng

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
