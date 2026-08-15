// ==========================================================================
// MODULE: OrderEmailTemplateHelper.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module OrderEmailTemplateHelper
// ==========================================================================
using ECommerce.Models;
using System;
using System.Linq;
using System.Text;

namespace ECommerce1.Services
{
    public static class OrderEmailTemplateHelper
    {
        private static string GetHeader(string title)
        {
            return $@"
            <div style='font-family: Arial, Helvetica, sans-serif; max-width: 600px; margin: 0 auto; padding: 0; background-color: #f8fafc; border-radius: 12px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);'>
                <div style='background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%); padding: 24px 20px; text-align: center;'>
                    <h1 style='color: #ffffff; margin: 0; font-size: 26px; font-weight: 800; letter-spacing: 0.5px;'>PhoneStore</h1>
                    <p style='color: #dbeafe; margin: 6px 0 0 0; font-size: 14px; font-weight: 600;'>{title}</p>
                </div>
                <div style='padding: 24px 20px; background-color: #ffffff;'>";
        }

        private static string GetFooter(string frontendBaseUrl)
        {
            return $@"
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 24px 0;'/>
                    <div style='text-align: center; color: #64748b; font-size: 12px; line-height: 1.6;'>
                        <p style='margin: 0 0 4px 0; font-weight: 600; color: #475569;'>Cảm ơn quý khách đã tin tưởng và đồng hành cùng PhoneStore!</p>
                        <p style='margin: 0;'>Mọi thắc mắc cần hỗ trợ khẩn cấp, vui lòng liên hệ Hotline: <strong style='color: #2563eb;'>1900 6789</strong> hoặc Email: <a href='mailto:support@phonestore.vn' style='color: #2563eb; text-decoration: none;'>support@phonestore.vn</a></p>
                        <p style='margin: 8px 0 0 0; font-size: 11px; color: #94a3b8;'>© {DateTime.Now.Year} PhoneStore. All rights reserved.</p>
                    </div>
                </div>
            </div>";
        }

        // Bảng thông tin giao nhận & Đơn hàng (Bao gồm Thời gian đặt hàng)
        private static string BuildOrderInfoBox(Order order)
        {
            string orderTime = order.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
            string carrierName = string.IsNullOrWhiteSpace(order.ShippingCarrier) ? "Giao hàng tiêu chuẩn" : order.ShippingCarrier;

            return $@"
            <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-left: 4px solid #2563eb; padding: 16px; margin: 16px 0; border-radius: 6px; font-size: 13px; color: #1e293b; line-height: 1.7;'>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 3px 0; font-weight: bold; color: #475569; width: 40%;'>🔖 Mã đơn hàng:</td>
                        <td style='padding: 3px 0; font-weight: bold; color: #1e40af;'>#{order.Id}</td>
                    </tr>
                    <tr>
                        <td style='padding: 3px 0; font-weight: bold; color: #475569;'>⏰ Thời gian đặt hàng:</td>
                        <td style='padding: 3px 0; font-weight: 600; color: #0f172a;'>{orderTime}</td>
                    </tr>
                    <tr>
                        <td style='padding: 3px 0; font-weight: bold; color: #475569;'>👤 Người nhận:</td>
                        <td style='padding: 3px 0; font-weight: 600; color: #0f172a;'>{order.ReceiverName} ({order.ReceiverPhone})</td>
                    </tr>
                    <tr>
                        <td style='padding: 3px 0; font-weight: bold; color: #475569;'>📍 Địa chỉ nhận hàng:</td>
                        <td style='padding: 3px 0; font-weight: 500; color: #0f172a;'>{order.ShippingAddressLine}, {order.ShippingWard}, {order.ShippingProvince}</td>
                    </tr>
                    <tr>
                        <td style='padding: 3px 0; font-weight: bold; color: #475569;'>💳 Thanh toán:</td>
                        <td style='padding: 3px 0; font-weight: bold; color: #16a34a;'>{order.PaymentMethod?.ToUpper()}</td>
                    </tr>
                    <tr>
                        <td style='padding: 3px 0; font-weight: bold; color: #475569;'>🚚 Vận chuyển:</td>
                        <td style='padding: 3px 0; font-weight: 600; color: #0f172a;'>{carrierName}</td>
                    </tr>
                </table>
            </div>";
        }

        // Bảng danh sách sản phẩm
        private static string BuildOrderItemsTable(Order order)
        {
            var sb = new StringBuilder();
            sb.Append(@"
            <table style='width: 100%; border-collapse: collapse; margin-top: 16px; font-size: 13px;'>
                <thead>
                    <tr style='background-color: #f1f5f9; color: #334155; text-align: left;'>
                        <th style='padding: 10px 12px; border-bottom: 2px solid #cbd5e1; border-top-left-radius: 6px;'>Sản phẩm</th>
                        <th style='padding: 10px 8px; border-bottom: 2px solid #cbd5e1; text-align: center;'>Số lượng</th>
                        <th style='padding: 10px 12px; border-bottom: 2px solid #cbd5e1; text-align: right; border-top-right-radius: 6px;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>");

            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    string productName = item.ProductVariant?.Product?.Name ?? "Sản phẩm";
                    string variantName = (item.ProductVariant != null && item.ProductVariant.Name != "Mặc định") 
                        ? $" <span style='color: #64748b; font-size: 11px;'>({item.ProductVariant.Name})</span>" 
                        : "";
                    string warrantyInfo = item.Warranty != null 
                        ? $"<br/><span style='color: #2563eb; font-size: 11px; font-weight: bold;'>🛡️ BH: {item.Warranty.Name} (+{item.WarrantyPrice:N0}đ)</span>" 
                        : "";

                    decimal itemTotal = item.Quantity * (item.PriceAtPurchase + item.WarrantyPrice);

                    sb.Append($@"
                    <tr style='border-bottom: 1px solid #f1f5f9;'>
                        <td style='padding: 10px 12px; color: #1e293b; font-weight: 600;'>
                            {productName}{variantName}{warrantyInfo}
                        </td>
                        <td style='padding: 10px 8px; text-align: center; color: #475569; font-weight: bold;'>{item.Quantity}</td>
                        <td style='padding: 10px 12px; text-align: right; color: #0f172a; font-weight: bold;'>{itemTotal:N0}đ</td>
                    </tr>");
                }
            }

            sb.Append(@"
                </tbody>
            </table>");

            return sb.ToString();
        }

        // Bảng tổng quan chi phí & các khoản giảm giá (Combo, Voucher, Điểm tích lũy, Phí ship)
        private static string BuildOrderPriceSummaryTable(Order order)
        {
            decimal itemsSubtotal = order.OrderItems?.Sum(i => i.Quantity * (i.PriceAtPurchase + i.WarrantyPrice)) ?? 0;
            decimal comboDiscount = order.OrderItems?.Sum(i => i.CampaignDiscountAmount * i.Quantity) ?? 0;
            decimal pointsDiscount = order.DiscountFromPoints;
            decimal shippingFee = order.ActualShippingFee ?? 0;

            // Tính khoản giảm từ Voucher (nếu có mã giảm giá)
            decimal promoDiscount = 0;
            if (order.Promotion != null)
            {
                // Tổng chi trả = Subtotal - ComboDiscount - PromoDiscount - PointsDiscount + ShippingFee
                promoDiscount = itemsSubtotal - comboDiscount - pointsDiscount + shippingFee - order.TotalPrice;
                if (promoDiscount < 0) promoDiscount = 0;
            }

            var sb = new StringBuilder();
            sb.Append(@"
            <div style='margin-top: 16px; padding: 16px; background-color: #f8fafc; border-radius: 8px; border: 1px solid #e2e8f0; font-size: 13px;'>
                <table style='width: 100%; border-collapse: collapse; line-height: 1.8;'>
                    <tr>
                        <td style='color: #475569; font-weight: 600;'>Tạm tính tiền hàng:</td>
                        <td style='text-align: right; font-weight: bold; color: #1e293b;'>")
              .Append($"{itemsSubtotal:N0}đ</td></tr>");

            if (comboDiscount > 0)
            {
                sb.Append(@"
                    <tr>
                        <td style='color: #2563eb; font-weight: 600;'>🔥 Ưu đãi Combo / Giảm giá SP:</td>
                        <td style='text-align: right; font-weight: bold; color: #2563eb;'>")
                  .Append($"-{comboDiscount:N0}đ</td></tr>");
            }

            if (promoDiscount > 0 && order.Promotion != null)
            {
                sb.Append(@"
                    <tr>
                        <td style='color: #dc2626; font-weight: 600;'>🎟️ Mã giảm giá (")
                  .Append(order.Promotion.Code)
                  .Append(@"):</td>
                        <td style='text-align: right; font-weight: bold; color: #dc2626;'>")
                  .Append($"-{promoDiscount:N0}đ</td></tr>");
            }

            if (pointsDiscount > 0)
            {
                sb.Append(@"
                    <tr>
                        <td style='color: #ca8a04; font-weight: 600;'>🎁 Giảm giá tích điểm (")
                  .Append(order.PointsRedeemed)
                  .Append(@" điểm):</td>
                        <td style='text-align: right; font-weight: bold; color: #ca8a04;'>")
                  .Append($"-{pointsDiscount:N0}đ</td></tr>");
            }

            string shippingText = shippingFee > 0 ? $"+{shippingFee:N0}đ" : "<span style='color: #16a34a; font-weight: bold;'>Miễn phí</span>";
            sb.Append(@"
                    <tr>
                        <td style='color: #475569; font-weight: 600;'>🚚 Phí vận chuyển:</td>
                        <td style='text-align: right; font-weight: bold; color: #1e293b;'>")
              .Append(shippingText)
              .Append(@"</td></tr>
                    <tr style='border-top: 2px solid #cbd5e1;'>
                        <td style='padding-top: 8px; font-size: 15px; font-weight: bold; color: #0f172a;'>TỔNG THÀNH TIỀN:</td>
                        <td style='padding-top: 8px; text-align: right; font-size: 17px; font-weight: 800; color: #dc2626;'>")
              .Append($"{order.TotalPrice:N0}đ</td></tr></table></div>");

            return sb.ToString();
        }

        // 0. Mail Đặt hàng thành công (Ngay sau khi checkout)
        public static string GetOrderPlacedEmailHtml(Order order, string frontendBaseUrl)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);
            string trackingUrl = $"{frontendBaseUrl.TrimEnd('/')}/order-tracking?orderId={order.Id}";

            return $@"
            {GetHeader("ĐẶT HÀNG THÀNH CÔNG")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    Cảm ơn bạn đã đặt hàng tại <strong>PhoneStore</strong>! Đơn hàng của bạn đã được hệ thống ghi nhận thành công và đang chờ nhân viên kiểm duyệt.
                </p>

                {infoBox}

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Danh sách sản phẩm đã đặt:</h3>
                {itemsTable}

                {priceSummary}

                <div style='text-align: center; margin-top: 28px;'>
                    <a href='{trackingUrl}' target='_blank' style='background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); color: #ffffff; padding: 12px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 14px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(37,99,235,0.3);'>
                        🔍 Tra cứu tiến trình đơn hàng #{order.Id}
                    </a>
                </div>
            {GetFooter(frontendBaseUrl)}";
        }

        // 1. Mail Xác nhận / Duyệt đơn (confirmed / preparing)
        public static string GetOrderConfirmedEmailHtml(Order order, string frontendBaseUrl)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);
            string trackingUrl = $"{frontendBaseUrl.TrimEnd('/')}/order-tracking?orderId={order.Id}";

            return $@"
            {GetHeader("XÁC NHẬN VÀ ĐANG CHUẨN BỊ ĐƠN HÀNG")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    PhoneStore trân trọng thông báo: Đơn hàng <strong>#{order.Id}</strong> của bạn đã được nhân viên kiểm duyệt thành công và hiện đang trong quá trình đóng gói để chuẩn bị bàn giao cho đối tác vận chuyển!
                </p>

                {infoBox}

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Danh sách sản phẩm trong đơn:</h3>
                {itemsTable}

                {priceSummary}

                <div style='text-align: center; margin-top: 28px;'>
                    <a href='{trackingUrl}' target='_blank' style='background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); color: #ffffff; padding: 12px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 14px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(37,99,235,0.3);'>
                        🔍 Theo dõi trạng thái đơn hàng #{order.Id}
                    </a>
                </div>
            {GetFooter(frontendBaseUrl)}";
        }

        // 2. Mail Đang giao hàng (shipping)
        public static string GetOrderShippingEmailHtml(Order order, string frontendBaseUrl)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string carrierName = string.IsNullOrWhiteSpace(order.ShippingCarrier) ? "Đơn vị vận chuyển liên kết" : order.ShippingCarrier;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);
            string trackingUrl = $"{frontendBaseUrl.TrimEnd('/')}/order-tracking?orderId={order.Id}";

            string ahamoveSection = "";
            if (!string.IsNullOrWhiteSpace(order.AhamoveSharedLink))
            {
                ahamoveSection = $@"
                <div style='background-color: #fff7ed; border: 1px solid #ffedd5; padding: 16px; margin: 16px 0; border-radius: 8px; text-align: center;'>
                    <p style='margin: 0 0 10px 0; color: #c2410c; font-size: 13px; font-weight: bold;'>⚡ Đơn hàng đang được tài xế Ahamove giao siêu tốc!</p>
                    <a href='{order.AhamoveSharedLink}' target='_blank' style='background-color: #ea580c; color: #ffffff; padding: 10px 20px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 13px; display: inline-block;'>
                        🛵 Bấm vào đây để theo dõi hành trình tài xế ↗
                    </a>
                </div>";
            }

            return $@"
            {GetHeader("ĐƠN HÀNG ĐANG ĐƯỢC GIAO")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    Đơn hàng <strong>#{order.Id}</strong> của bạn đã được xuất kho và bàn giao cho đối tác <strong>{carrierName}</strong>. Tài xế/giao hàng viên đang trên đường mang sản phẩm tới địa chỉ của bạn!
                </p>

                {infoBox}

                {ahamoveSection}

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Danh sách sản phẩm đang giao:</h3>
                {itemsTable}

                {priceSummary}

                <div style='text-align: center; margin-top: 24px;'>
                    <a href='{trackingUrl}' target='_blank' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 14px; display: inline-block;'>
                        📦 Theo dõi chi tiết đơn hàng trên Web
                    </a>
                </div>
            {GetFooter(frontendBaseUrl)}";
        }

        // 3. Mail Giao hàng thành công (delivered)
        public static string GetOrderDeliveredEmailHtml(Order order, string frontendBaseUrl)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);
            string reviewUrl = $"{frontendBaseUrl.TrimEnd('/')}/order-tracking?orderId={order.Id}";
            
            string pointsSection = "";
            if (order.PointsEarned > 0)
            {
                pointsSection = $@"
                <div style='background-color: #fefce8; border: 1px dashed #ca8a04; padding: 12px 16px; margin: 16px 0; border-radius: 8px; text-align: center; font-size: 13px; color: #854d0e;'>
                    🎁 <strong>Chúc mừng!</strong> Bạn đã tích lũy thành công <strong style='color: #16a34a; font-size: 15px;'>+{order.PointsEarned} điểm thưởng</strong> vào tài khoản thành viên PhoneStore!
                </div>";
            }

            return $@"
            {GetHeader("GIAO HÀNG THÀNH CÔNG - CẢM ƠN QUÝ KHÁCH!")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    Đơn hàng <strong>#{order.Id}</strong> của bạn đã được giao thành công! PhoneStore xin chân thành cảm ơn sự tin tưởng và lựa chọn mua sắm của bạn.
                </p>

                {infoBox}

                {pointsSection}

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Chi tiết đơn hàng hoàn tất:</h3>
                {itemsTable}

                {priceSummary}

                <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; padding: 16px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                    <h4 style='margin: 0 0 8px 0; color: #1e293b; font-size: 14px;'>Ý kiến của bạn rất quan trọng đối với PhoneStore ⭐</h4>
                    <p style='margin: 0 0 14px 0; color: #64748b; font-size: 12px;'>Hãy dành 1 phút để đánh giá chất lượng sản phẩm và dịch vụ để giúp chúng tôi phục vụ bạn tốt hơn trong tương lai!</p>
                    <a href='{reviewUrl}' target='_blank' style='background: linear-gradient(135deg, #16a34a 0%, #15803d 100%); color: #ffffff; padding: 11px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 13px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(22,163,74,0.3);'>
                        ⭐ Viết đánh giá sản phẩm ngay
                    </a>
                </div>
            {GetFooter(frontendBaseUrl)}";
        }

        // 4. Mail Hủy đơn hàng (cancelled)
        public static string GetOrderCancelledEmailHtml(Order order, string frontendBaseUrl, string? cancelReason = null)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);

            string reasonText = string.IsNullOrWhiteSpace(cancelReason) 
                ? "Theo yêu cầu từ phía khách hàng hoặc thay đổi thông tin đơn." 
                : cancelReason;

            string isPrepaid = (order.PaymentMethod != null && order.PaymentMethod.ToLower() != "cod")
                ? " (Đã thanh toán Online)"
                : "";

            string refundInfo = (order.PaymentMethod != null && order.PaymentMethod.ToLower() != "cod")
                ? $@"<div style='background-color: #fff1f2; border-left: 4px solid #e11d48; padding: 12px 16px; margin: 16px 0; border-radius: 4px; font-size: 13px; color: #9f1239;'>
                        <p style='margin: 0 0 4px 0;'><strong>💳 Thông tin hoàn tiền:</strong> Đơn hàng đã được thanh toán qua {order.PaymentMethod}. Bộ phận Kế toán của PhoneStore sẽ tiến hành thủ tục hoàn trả số tiền <strong>{order.TotalPrice:N0}đ</strong> về tài khoản/thẻ của quý khách trong vòng 2-5 ngày làm việc.</p>
                     </div>"
                : "";

            return $@"
            {GetHeader("THÔNG BÁO HỦY ĐƠN HÀNG")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    PhoneStore xin thông báo: Đơn hàng <strong>#{order.Id}</strong>{isPrepaid} của bạn đã được hủy thành công trên hệ thống.
                </p>

                {infoBox}

                <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; padding: 14px 16px; margin: 16px 0; border-radius: 6px; font-size: 13px; color: #475569;'>
                    <p style='margin: 0;'><strong>📝 Lý do hủy đơn:</strong> {reasonText}</p>
                </div>

                {refundInfo}

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Chi tiết đơn hàng đã hủy:</h3>
                {itemsTable}

                {priceSummary}

                <p style='color: #475569; font-size: 13px; line-height: 1.6; margin-top: 16px;'>
                    Nếu đây là sự nhầm lẫn hoặc bạn muốn mua sản phẩm khác, xin vui lòng truy cập lại website PhoneStore để đặt đơn hàng mới. Chúng tôi rất hân hạnh được phục vụ quý khách!
                </p>
            {GetFooter(frontendBaseUrl)}";
        }

        // 5. Mail Đổi trả / Hoàn tiền (refunded)
        public static string GetOrderRefundedEmailHtml(Order order, string frontendBaseUrl, string? refundNote = null)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);

            string noteText = string.IsNullOrWhiteSpace(refundNote) 
                ? "Đã xử lý hoàn tiền theo thỏa thuận đổi trả / trả hàng thành công." 
                : refundNote;

            return $@"
            {GetHeader("XÁC NHẬN HOÀN TIỀN THÀNH CÔNG")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    PhoneStore xác nhận đã hoàn tất thủ tục hoàn tiền cho đơn hàng <strong>#{order.Id}</strong> của bạn.
                </p>

                {infoBox}

                <div style='background-color: #f3e8ff; border-left: 4px solid #9333ea; padding: 14px 16px; margin: 16px 0; border-radius: 4px; font-size: 13px; color: #6b21a8;'>
                    <p style='margin: 0 0 6px 0;'><strong>💰 Số tiền hoàn trả:</strong> <strong style='font-size: 16px; color: #7e22ce;'>{order.TotalPrice:N0}đ</strong></p>
                    <p style='margin: 0;'><strong>📌 Ghi chú xử lý:</strong> {noteText}</p>
                </div>

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Chi tiết đơn hàng hoàn tiền:</h3>
                {itemsTable}

                {priceSummary}

                <p style='color: #475569; font-size: 13px; line-height: 1.6;'>
                    Tiền sẽ được chuyển về tài khoản/phương thức thanh toán ban đầu của quý khách. Xin cảm ơn sự kiên nhẫn và hợp tác của bạn!
                </p>
            {GetFooter(frontendBaseUrl)}";
        }

        // 6. Mail Giao hàng thất bại (shipping_failed)
        public static string GetOrderShippingFailedEmailHtml(Order order, string frontendBaseUrl, int failedCount = 1)
        {
            string receiverName = string.IsNullOrWhiteSpace(order.ReceiverName) ? "Quý khách" : order.ReceiverName;
            string infoBox = BuildOrderInfoBox(order);
            string itemsTable = BuildOrderItemsTable(order);
            string priceSummary = BuildOrderPriceSummaryTable(order);
            string trackingUrl = $"{frontendBaseUrl.TrimEnd('/')}/order-tracking?orderId={order.Id}";

            return $@"
            {GetHeader("THÔNG BÁO GIAO HÀNG KHÔNG THÀNH CÔNG")}
                <p style='font-size: 15px; color: #1e293b; margin-top: 0;'>Kính chào <strong>{receiverName}</strong>,</p>
                <p style='color: #334155; font-size: 14px; line-height: 1.6;'>
                    Đối tác vận chuyển báo cáo việc giao đơn hàng <strong>#{order.Id}</strong> đến địa chỉ của bạn chưa thành công (Lần {failedCount}/3).
                </p>

                {infoBox}

                <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 14px 16px; margin: 16px 0; border-radius: 4px; font-size: 13px; color: #991b1b;'>
                    <p style='margin: 0 0 6px 0;'><strong>⚠️ Lý do có thể:</strong> Không liên lạc được qua số điện thoại hoặc người nhận vắng nhà.</p>
                    <p style='margin: 0;'><strong>📍 Địa chỉ nhận hàng:</strong> {order.ShippingAddressLine}, {order.ShippingWard}, {order.ShippingProvince}</p>
                </div>

                <h3 style='font-size: 14px; color: #1e293b; margin: 20px 0 8px 0;'>Chi tiết sản phẩm đơn hàng:</h3>
                {itemsTable}

                {priceSummary}

                <p style='color: #475569; font-size: 13px; line-height: 1.6;'>
                    Nhân viên giao hàng sẽ tiến hành liên hệ lại để giao lại cho bạn. Nếu cần thay đổi giờ nhận hoặc số điện thoại, vui lòng liên hệ ngay với Hotline <strong>1900 6789</strong> để được hỗ trợ kịp thời!
                </p>

                <div style='text-align: center; margin-top: 24px;'>
                    <a href='{trackingUrl}' target='_blank' style='background-color: #dc2626; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 14px; display: inline-block;'>
                        📞 Xem thông tin đơn hàng & Hỗ trợ
                    </a>
                </div>
            {GetFooter(frontendBaseUrl)}";
        }
    }
}
