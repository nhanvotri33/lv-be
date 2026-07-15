using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace ECommerce1.Services
{
    public class AhamoveService : IAhamoveService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private const string CacheTokenKey = "AhamoveAccessToken";

        public AhamoveService(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_memoryCache.TryGetValue(CacheTokenKey, out string cachedToken))
            {
                return cachedToken;
            }

            var apiKey = _configuration["Ahamove:ApiKey"];
            var mobile = _configuration["Ahamove:Mobile"];
            var baseUrl = _configuration["Ahamove:BaseUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(mobile))
            {
                throw new InvalidOperationException("Cấu hình Ahamove (ApiKey, Mobile) không hợp lệ hoặc thiếu.");
            }

            // Endpoint đăng ký/đăng nhập lấy token của Ahamove
            var requestUrl = $"{baseUrl}/v1/partner/register_account?mobile={mobile}&name=ECommerceStore&api_key={apiKey}";
            
            var response = await _httpClient.GetAsync(requestUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Lỗi xác thực Ahamove: {response.StatusCode} - {content}");
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("token", out var tokenProp))
            {
                var token = tokenProp.GetString();
                // Cache token trong 23 giờ (token thường có thời hạn 24 giờ)
                _memoryCache.Set(CacheTokenKey, token, TimeSpan.FromHours(23));
                return token;
            }

            throw new InvalidOperationException($"Không tìm thấy trường 'token' trong phản hồi của Ahamove: {content}");
        }

        public async Task<decimal> EstimateFeeAsync(double destLat, double destLng, string destAddress, string serviceId = "SGN-BIKE")
        {
            var token = await GetTokenAsync();
            var baseUrl = _configuration["Ahamove:BaseUrl"];
            
            var warehouseAddress = _configuration["Ahamove:WarehouseAddress"] ?? "180 Cao Lỗ, Phường 4, Quận 8, Hồ Chí Minh";
            var warehouseLat = double.Parse(_configuration["Ahamove:WarehouseLat"] ?? "10.7379415");
            var warehouseLng = double.Parse(_configuration["Ahamove:WarehouseLng"] ?? "106.6757237");

            var pathList = new List<object>
            {
                new { lat = warehouseLat, lng = warehouseLng, address = warehouseAddress },
                new { lat = destLat, lng = destLng, address = destAddress }
            };

            var pathJson = JsonSerializer.Serialize(pathList);

            var postData = new Dictionary<string, string>
            {
                { "token", token },
                { "order_time", "0" },
                { "service_id", serviceId },
                { "path", pathJson }
            };

            var requestUrl = $"{baseUrl}/v1/order/estimated_fee";
            var response = await _httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(postData));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Lỗi tính phí Ahamove: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("total_fee", out var feeProp))
            {
                return (decimal)feeProp.GetDouble();
            }

            throw new InvalidOperationException($"Phản hồi từ Ahamove không hợp lệ: {content}");
        }

        public async Task<AhamoveOrderResponse> CreateOrderAsync(Order order, string serviceId = "SGN-BIKE")
        {
            var token = await GetTokenAsync();
            var baseUrl = _configuration["Ahamove:BaseUrl"];

            var warehouseAddress = _configuration["Ahamove:WarehouseAddress"] ?? "180 Cao Lỗ, Phường 4, Quận 8, Hồ Chí Minh";
            var warehouseLat = double.Parse(_configuration["Ahamove:WarehouseLat"] ?? "10.7379415");
            var warehouseLng = double.Parse(_configuration["Ahamove:WarehouseLng"] ?? "106.6757237");
            var shopMobile = _configuration["Ahamove:Mobile"] ?? "0797200168";

            if (!order.DeliveryLatitude.HasValue || !order.DeliveryLongitude.HasValue)
            {
                throw new ArgumentException("Đơn hàng chưa có tọa độ điểm giao hàng để gửi Ahamove.");
            }

            var destAddress = $"{order.ShippingAddressLine}, {order.ShippingWard}, {order.ShippingProvince}";
            var codAmount = order.PaymentMethod.ToUpper() == "COD" ? (double)order.TotalPrice : 0.0;

            // Xây dựng path JSON: Điểm 0 (kho shop) -> Điểm 1 (khách hàng)
            var pathList = new List<object>
            {
                new { lat = warehouseLat, lng = warehouseLng, address = warehouseAddress, name = "Cửa hàng E-Commerce", mobile = shopMobile },
                new { lat = order.DeliveryLatitude.Value, lng = order.DeliveryLongitude.Value, address = destAddress, name = order.ReceiverName, mobile = order.ReceiverPhone, cod = codAmount }
            };

            var pathJson = JsonSerializer.Serialize(pathList);

            // Xây dựng items JSON
            var itemsList = new List<object>
            {
                new { _id = $"order_{order.Id}", num = 1, name = $"Đơn hàng #{order.Id}", price = (double)order.TotalPrice }
            };
            var itemsJson = JsonSerializer.Serialize(itemsList);

            var postData = new Dictionary<string, string>
            {
                { "token", token },
                { "service_id", serviceId },
                { "payment_method", "BALANCE" }, // Trừ phí từ số dư tài khoản của Shop
                { "order_time", "0" },
                { "path", pathJson },
                { "items", itemsJson }
            };

            var requestUrl = $"{baseUrl}/v1/order/create";
            var response = await _httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(postData));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Lỗi tạo đơn Ahamove: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            string orderId = root.TryGetProperty("order_id", out var idProp) ? idProp.GetString() : string.Empty;
            string status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "ASSIGNING";
            string sharedLink = root.TryGetProperty("shared_link", out var linkProp) ? linkProp.GetString() : string.Empty;
            decimal totalFee = 0;

            // Lấy phí ship thực tế
            if (root.TryGetProperty("fee", out var feeProp))
            {
                totalFee = (decimal)feeProp.GetDouble();
            }

            return new AhamoveOrderResponse
            {
                OrderId = orderId,
                Status = status,
                SharedLink = sharedLink,
                TotalFee = totalFee
            };
        }
    }
}
