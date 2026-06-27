namespace ECommerce1.DTOs.Order
{
    public class CheckoutRequest
    {
        public int? ShippingInfoId { get; set; } // Nếu đã có địa chỉ từ trước
        
        // Nếu tạo địa chỉ mới
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine { get; set; }
        public string? WardId { get; set; }

        public string? PromotionCode { get; set; } // Mã giảm giá (nếu có)
        public string? PaymentMethod { get; set; } // COD, Stripe, Momo, etc.
        public int PointsToRedeem { get; set; } = 0;
        public string? Note { get; set; } // Ghi chú giao hàng
    }
}
