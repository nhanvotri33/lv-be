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

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(mobile) || string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("Cấu hình Ahamove (ApiKey, Mobile, BaseUrl) không hợp lệ hoặc thiếu.");
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
            try
            {
                var token = await GetTokenAsync();
                var baseUrl = _configuration["Ahamove:BaseUrl"];
                
                var warehouseAddress = _configuration["Ahamove:WarehouseAddress"] ?? "180 Cao Lỗ, Phường 4, Quận 8, Hồ Chí Minh";
                var warehouseLatVal = double.Parse(_configuration["Ahamove:WarehouseLat"] ?? "10.7379415");
                var warehouseLngVal = double.Parse(_configuration["Ahamove:WarehouseLng"] ?? "106.6757237");

                var pathList = new List<object>
                {
                    new { lat = warehouseLatVal, lng = warehouseLngVal, address = warehouseAddress },
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

                // Nếu token bị 401 / hết hạn -> Xóa cache token và thử lại với token mới
                if (!response.IsSuccessStatusCode && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || content.Contains("NOT_AUTHORIZED", StringComparison.OrdinalIgnoreCase)))
                {
                    _memoryCache.Remove(CacheTokenKey);
                    token = await GetTokenAsync();
                    postData["token"] = token;
                    response = await _httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(postData));
                    content = await response.Content.ReadAsStringAsync();
                }

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("total_price", out var totalPriceProp) && totalPriceProp.GetDouble() > 0)
                    {
                        return (decimal)totalPriceProp.GetDouble();
                    }
                    if (root.TryGetProperty("subtotal_price", out var subtotalProp) && subtotalProp.GetDouble() > 0)
                    {
                        return (decimal)subtotalProp.GetDouble();
                    }
                    if (root.TryGetProperty("distance_price", out var distPriceProp) && distPriceProp.GetDouble() > 0)
                    {
                        return (decimal)distPriceProp.GetDouble();
                    }
                    if (root.TryGetProperty("total_fee", out var feeProp))
                    {
                        return (decimal)feeProp.GetDouble();
                    }
                }
                else
                {
                    Console.WriteLine($"Ahamove API returned non-success: {content}. Falling back to simulation.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ahamove API exception: {ex.Message}. Falling back to simulation.");
            }

            // Fallback: Giả lập tính phí Ahamove dựa trên khoảng cách địa lý đơn giản từ kho (Quận 8 Cao Lỗ) đến điểm giao hàng
            double warehouseLat = 10.7379415;
            double warehouseLng = 106.6757237;
            
            double dLat = destLat - warehouseLat;
            double dLng = destLng - warehouseLng;
            double distanceKm = Math.Sqrt(dLat * dLat + dLng * dLng) * 111.0;
            
            if (distanceKm < 2) distanceKm = 2; // Tối thiểu 2km
            
            decimal baseFee = serviceId switch
            {
                "SGN-BIKE" => 15000m,       // Giao Siêu Tốc: 15k cho 3km đầu
                "SGN-EXPRESS" => 18000m,    // Siêu Tốc Tiết Kiệm: 18k cho 3km đầu
                "SGN-2H" => 12000m,         // Giao 2H: 12k cho 3km đầu
                "SGN-POOL" => 12000m,       // Giao 4H: 12k cho 3km đầu
                _ => 15000m
            };
            
            decimal perKmFee = serviceId switch
            {
                "SGN-BIKE" => 5000m,        // 5k mỗi km tiếp theo
                "SGN-EXPRESS" => 4000m,     // 4k mỗi km tiếp theo
                "SGN-2H" => 3000m,          // 3k mỗi km tiếp theo
                "SGN-POOL" => 3000m,        // 3k mỗi km tiếp theo
                _ => 4000m
            };
            
            decimal calculatedFee = baseFee;
            if (distanceKm > 3)
            {
                calculatedFee += (decimal)(distanceKm - 3) * perKmFee;
            }
            
            if (calculatedFee < baseFee) calculatedFee = baseFee;
            
            // Làm tròn phí đến hàng nghìn đồng
            return Math.Round(calculatedFee / 1000m) * 1000m;
        }

        public async Task<AhamoveOrderResponse> CreateOrderAsync(Order order, string serviceId = "SGN-BIKE")
        {
            try
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

                var pathList = new List<object>
                {
                    new { lat = warehouseLat, lng = warehouseLng, address = warehouseAddress, name = "Cửa hàng E-Commerce", mobile = shopMobile },
                    new { lat = order.DeliveryLatitude.Value, lng = order.DeliveryLongitude.Value, address = destAddress, name = order.ReceiverName, mobile = order.ReceiverPhone, cod = codAmount }
                };

                var pathJson = JsonSerializer.Serialize(pathList);

                var itemsList = new List<object>
                {
                    new { _id = $"order_{order.Id}", num = 1, name = $"Đơn hàng #{order.Id}", price = (double)order.TotalPrice }
                };
                var itemsJson = JsonSerializer.Serialize(itemsList);

                var postData = new Dictionary<string, string>
                {
                    { "token", token },
                    { "service_id", serviceId },
                    { "payment_method", "BALANCE" },
                    { "order_time", "0" },
                    { "path", pathJson },
                    { "items", itemsJson }
                };

                var requestUrl = $"{baseUrl}/v1/order/create";
                var response = await _httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(postData));
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || content.Contains("NOT_AUTHORIZED", StringComparison.OrdinalIgnoreCase)))
                {
                    _memoryCache.Remove(CacheTokenKey);
                    token = await GetTokenAsync();
                    postData["token"] = token;
                    response = await _httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(postData));
                    content = await response.Content.ReadAsStringAsync();
                }

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    string orderId = root.TryGetProperty("order_id", out var idProp) ? idProp.GetString() : string.Empty;
                    string status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "ASSIGNING";
                    string sharedLink = root.TryGetProperty("shared_link", out var linkProp) ? linkProp.GetString() : string.Empty;
                    decimal totalFee = 0;

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
                else
                {
                    Console.WriteLine($"Ahamove CreateOrder returned non-success: {content}. Falling back to simulation.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ahamove CreateOrder exception: {ex.Message}. Falling back to simulation.");
            }

            // Fallback: Giả lập tạo đơn
            return new AhamoveOrderResponse
            {
                OrderId = $"MOCK_AHA_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Status = "ASSIGNING",
                SharedLink = "https://track.ahamove.com/mock-tracking-link",
                TotalFee = serviceId switch
                {
                    "SGN-BIKE" => 34000m,
                    "SGN-EXPRESS" => 36000m,
                    "SGN-2H" => 30000m,
                    _ => 25000m
                }
            };
        }
    }
}
