// ==========================================================================
// MODULE: OrderResponse.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module OrderResponse
// ==========================================================================
using System;
using System.Collections.Generic;
using System.Linq;

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
        // Số tiền giảm của MÃ GIẢM GIÁ cấp đơn. Không lưu cột riêng trong CSDL nên được suy ra
        // từ các số liệu đã chốt lúc đặt hàng, theo đúng công thức của OrderService.CheckoutAsync:
        //   TotalPrice = SubTotal - promo - DiscountFromPoints + ActualShippingFee
        public decimal DiscountApplied => Math.Max(0m,
            Math.Round(SubTotal - DiscountFromPoints + (ActualShippingFee ?? 0m) - TotalPrice, 2));
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal DiscountFromPoints { get; set; }
        public string? Note { get; set; } // Ghi chú giao hàng
        public string? ShippingCarrier { get; set; } // Đơn vị vận chuyển (Ahamove, GHN, v.v...)
        
        // --- Ahamove Integration ---
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public string? AhamoveOrderId { get; set; }
        public string? AhamoveStatus { get; set; }
        public string? AhamoveSharedLink { get; set; }
        public decimal? ActualShippingFee { get; set; }

        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();

        // ================= BÁO CÁO LỢI NHUẬN (Doanh thu hàng - Giá vốn nhập kho) =================
        // Quy ước nghiệp vụ: CHỈ ghi nhận lợi nhuận thực tế với đơn hàng THÀNH CÔNG (StatusId = 4 - Đã giao).
        // Các đơn chưa giao/hủy chỉ hiển thị lợi nhuận DỰ KIẾN (EstimatedGrossProfit) để tham khảo.
        public bool IsProfitRealized => StatusId == 4;

        // Doanh thu hàng hóa (chưa gồm phí ship, chưa trừ khuyến mãi cấp đơn)
        public decimal TotalItemRevenue => Items.Sum(i => i.SubTotal);

        // ================= BÓC TÁCH TIỀN CỦA ĐƠN (khớp 1-1 với OrderService.CheckoutAsync) =================
        // Tổng tiền gói bảo hành đi kèm các dòng hàng
        public decimal TotalWarranty => Items.Sum(i => i.WarrantySubTotal);

        // Tạm tính: tiền hàng (đã trừ khuyến mãi combo) + gói bảo hành. Đây là mốc để áp mã giảm giá.
        public decimal SubTotal => TotalItemRevenue + TotalWarranty;

        // Tổng tiền được giảm nhờ khuyến mãi mua kèm (combo), tính theo snapshot lúc đặt hàng
        public decimal TotalComboDiscount => Items.Sum(i => i.ComboDiscountSubTotal);

        // Giá niêm yết gốc của toàn bộ hàng hóa, trước khi trừ khuyến mãi combo
        public decimal TotalOriginalItemRevenue => Items.Sum(i => i.OriginalSubTotal);

        // Tổng tiền gốc (giá nhập hàng) của toàn bộ dòng hàng trong đơn
        public decimal TotalCost => Items.Sum(i => i.CostSubTotal);

        // Lợi nhuận gộp dự kiến của đơn (áp dụng cho mọi trạng thái)
        public decimal EstimatedGrossProfit => TotalItemRevenue - TotalCost;

        // Lợi nhuận gộp THỰC TẾ: chỉ tính khi đơn đã giao thành công
        public decimal GrossProfit => IsProfitRealized ? EstimatedGrossProfit : 0m;

        // Biên lợi nhuận (%) trên doanh thu hàng hóa
        public double ProfitMargin => TotalItemRevenue > 0
            ? (double)Math.Round((EstimatedGrossProfit / TotalItemRevenue) * 100m, 2)
            : 0;
    }
}
