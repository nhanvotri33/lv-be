using System;
using System.Collections.Generic;

namespace ECommerce1.DTOs.Order
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string PromotionCode { get; set; }
        public decimal DiscountApplied { get; set; } // Số tiền được giảm giá
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal DiscountFromPoints { get; set; }
        public string? Note { get; set; } // Ghi chú giao hàng
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    }
}
