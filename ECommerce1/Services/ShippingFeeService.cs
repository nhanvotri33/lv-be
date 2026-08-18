// ==========================================================================
// MODULE: ShippingFeeService.cs
// MỤC ĐÍCH: NGUỒN DUY NHẤT tính phí vận chuyển.
//           Trước đây phí ship được tính ở 2 nơi với 2 bảng giá khác nhau:
//             - ShippingController.CalculateShippingFee  -> báo giá cho khách xem ở giỏ hàng
//             - OrderService.CheckoutAsync               -> tính lại lúc chốt đơn (thu tiền)
//           Hai bảng giá lệch nhau khiến số tiền khách thấy khác số tiền bị trừ.
//           Toàn bộ logic nay gom về đây để 2 luồng luôn ra cùng một con số.
// ==========================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class ShippingQuote
    {
        public decimal Fee { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string EstimatedDeliveryDays { get; set; } = string.Empty;
        // Mã dịch vụ Ahamove tương ứng (rỗng nếu là giao hàng tiêu chuẩn)
        public string AhamoveServiceId { get; set; } = string.Empty;
    }

    public interface IShippingFeeService
    {
        Task<List<ShippingQuote>> GetQuotesAsync(string provinceName, string wardName, string addressLine, double? latitude, double? longitude, decimal totalWeightKg);
        Task<ShippingQuote> ResolveQuoteAsync(string provinceName, string wardName, string addressLine, double? latitude, double? longitude, decimal totalWeightKg, string? selectedCarrier);
    }

    public class ShippingFeeService : IShippingFeeService
    {
        private readonly IAhamoveService _ahamoveService;

        // Kho hàng đặt tại Quận 8, TP.HCM nên Ahamove chỉ phục vụ được nội thành TP.HCM.
        private const double DefaultHcmLat = 10.776389;
        private const double DefaultHcmLng = 106.701139;

        // Danh mục dịch vụ Ahamove. Tên hiển thị (Carrier) chính là khoá để đối chiếu
        // lại lúc checkout, nên KHÔNG được sửa tên ở một nơi mà quên nơi kia.
        private static readonly (string ServiceId, string Carrier, string Days)[] AhamoveServices =
        {
            ("SGN-BIKE",    "Ahamove (Giao Siêu Tốc)",         "Trong vòng 1-2 giờ"),
            ("SGN-EXPRESS", "Ahamove (Siêu Tốc - Tiết Kiệm)",  "Trong vòng 2-4 giờ"),
            ("SGN-2H",      "Ahamove (Giao 2H - Tiết Kiệm)",   "Trong vòng 2 giờ")
        };

        public const string StandardCarrier = "Giao Hàng Tiêu Chuẩn";

        public ShippingFeeService(IAhamoveService ahamoveService)
        {
            _ahamoveService = ahamoveService;
        }

        private static bool IsHcm(string provinceName) =>
            provinceName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) ||
            provinceName.Contains("HCM", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Bảng giá giao hàng tiêu chuẩn. Đây là bảng giá DUY NHẤT được dùng cho cả
        /// lúc báo giá lẫn lúc chốt đơn.
        /// </summary>
        public static ShippingQuote GetStandardQuote(string provinceName, decimal totalWeightKg)
        {
            decimal baseFee = 45000;
            string days = "3-5 ngày";

            if (IsHcm(provinceName))
            {
                // Nội thành TP.HCM (gần kho)
                baseFee = 28000;
                days = "1-2 ngày";
            }
            else if (provinceName.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) ||
                     provinceName.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase) ||
                     provinceName.Contains("Hải Phòng", StringComparison.OrdinalIgnoreCase) ||
                     provinceName.Contains("Cần Thơ", StringComparison.OrdinalIgnoreCase))
            {
                baseFee = 38000;
                days = "2-3 ngày";
            }

            decimal weightMarkup = totalWeightKg > 2 ? (totalWeightKg - 2) * 5000 : 0;

            return new ShippingQuote
            {
                Fee = baseFee + weightMarkup,
                Carrier = StandardCarrier,
                EstimatedDeliveryDays = days
            };
        }

        /// <summary>
        /// Trả về toàn bộ tuỳ chọn vận chuyển khả dụng (Ahamove nếu ở TP.HCM + Giao hàng tiêu chuẩn).
        /// </summary>
        public async Task<List<ShippingQuote>> GetQuotesAsync(string provinceName, string wardName, string addressLine, double? latitude, double? longitude, decimal totalWeightKg)
        {
            var quotes = new List<ShippingQuote>();
            provinceName ??= "";

            if (IsHcm(provinceName))
            {
                double destLat = latitude.HasValue && latitude.Value != 0 ? latitude.Value : DefaultHcmLat;
                double destLng = longitude.HasValue && longitude.Value != 0 ? longitude.Value : DefaultHcmLng;
                string destAddress = BuildAddress(addressLine, wardName, provinceName);

                var tasks = AhamoveServices.Select(async s =>
                {
                    try
                    {
                        decimal fee = await _ahamoveService.EstimateFeeAsync(destLat, destLng, destAddress, s.ServiceId);
                        return new ShippingQuote
                        {
                            Fee = fee,
                            Carrier = s.Carrier,
                            EstimatedDeliveryDays = s.Days,
                            AhamoveServiceId = s.ServiceId
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi gọi dịch vụ Ahamove ({s.ServiceId}): {ex.Message}");
                        return null;
                    }
                });

                foreach (var q in await Task.WhenAll(tasks))
                {
                    if (q != null) quotes.Add(q);
                }
            }

            quotes.Add(GetStandardQuote(provinceName, totalWeightKg));
            return quotes;
        }

        /// <summary>
        /// Xác định phí phải thu cho đúng đơn vị vận chuyển khách đã chọn ở giỏ hàng.
        /// Đối chiếu theo TÊN đơn vị vận chuyển đã báo giá, không đoán theo từ khoá,
        /// nhờ vậy checkout luôn thu đúng con số đã hiện cho khách.
        /// </summary>
        public async Task<ShippingQuote> ResolveQuoteAsync(string provinceName, string wardName, string addressLine, double? latitude, double? longitude, decimal totalWeightKg, string? selectedCarrier)
        {
            provinceName ??= "";
            var standard = GetStandardQuote(provinceName, totalWeightKg);

            // Khách không chọn gì, hoặc chọn giao hàng tiêu chuẩn -> dùng bảng giá tiêu chuẩn
            if (string.IsNullOrWhiteSpace(selectedCarrier) ||
                !selectedCarrier.Contains("Ahamove", StringComparison.OrdinalIgnoreCase))
            {
                return standard;
            }

            var quotes = await GetQuotesAsync(provinceName, wardName, addressLine, latitude, longitude, totalWeightKg);

            var matched = quotes.FirstOrDefault(q =>
                q.Carrier.Equals(selectedCarrier, StringComparison.OrdinalIgnoreCase));

            // Không khớp được tên (khách đổi địa chỉ sau khi báo giá, hoặc Ahamove lỗi)
            // -> đổ về giao hàng tiêu chuẩn thay vì tự bịa ra một mức phí khác.
            return matched ?? standard;
        }

        private static string BuildAddress(string addressLine, string wardName, string provinceName)
        {
            return string.IsNullOrWhiteSpace(addressLine)
                ? $"{wardName}, {provinceName}"
                : $"{addressLine}, {wardName}, {provinceName}";
        }
    }
}
