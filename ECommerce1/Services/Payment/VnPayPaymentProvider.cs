// ==========================================================================
// MODULE: VnPayPaymentProvider.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module VnPayPaymentProvider
// ==========================================================================
using ECommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce1.Services.Payment
{
    public class VnPayPaymentProvider : IPaymentProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public VnPayPaymentProvider(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        public string ProviderName => "vnpay";

        // [Hàm thực thi nghiệp vụ]: `CreateCheckoutSessionAsync` - Xử lý logic và luồng dữ liệu
        public Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
        {
            var paymentUrl = _configuration["VnPay:PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var tmnCode = _configuration["VNPAY_TMN_CODE"] ?? _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VNPAY_HASH_SECRET"] ?? _configuration["VnPay:HashSecret"];

            if (string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret))
                throw new InvalidOperationException("VNPAY chưa được cấu hình TmnCode/HashSecret.");

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1")
                ipAddress = "127.0.0.1";

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = ((long)Math.Round(order.TotalPrice * 100, 0)).ToString(CultureInfo.InvariantCulture),
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = $"Thanh toan don hang #{order.Id}",
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = successUrl,
                ["vnp_TxnRef"] = order.Id.ToString(CultureInfo.InvariantCulture)
            };

            var query = BuildQueryString(vnpParams, encode: true);
            var hashData = BuildQueryString(vnpParams, encode: true);
            var secureHash = ComputeHmacSha512(hashData, hashSecret);

            return Task.FromResult($"{paymentUrl}?{query}&vnp_SecureHash={secureHash}");
        }

        // [Hàm thực thi nghiệp vụ]: `VerifySessionAsync` - Xử lý logic và luồng dữ liệu
        public Task<PaymentVerificationResult> VerifySessionAsync(string sessionId)
        {
            var query = _httpContextAccessor.HttpContext?.Request.Query;
            if (query == null || !query.ContainsKey("vnp_SecureHash"))
            {
                return Task.FromResult(new PaymentVerificationResult
                {
                    IsSuccess = false,
                    IsAuthentic = false,
                    TransactionId = string.Empty,
                    Message = "Thiếu dữ liệu callback VNPAY."
                });
            }

            var hashSecret = _configuration["VNPAY_HASH_SECRET"] ?? _configuration["VnPay:HashSecret"];
            if (string.IsNullOrWhiteSpace(hashSecret))
                throw new InvalidOperationException("VNPAY chưa được cấu hình HashSecret.");

            var secureHash = query["vnp_SecureHash"].ToString();
            var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var item in query)
            {
                if (!item.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (item.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                    item.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                    continue;

                data[item.Key] = item.Value.ToString();
            }

            var signedData = BuildQueryString(data, encode: true);
            var expectedHash = ComputeHmacSha512(signedData, hashSecret);
            if (!secureHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new PaymentVerificationResult
                {
                    IsSuccess = false,
                    IsAuthentic = false,
                    TransactionId = string.Empty,
                    Message = "Chữ ký VNPAY không hợp lệ."
                });
            }

            var responseCode = query["vnp_ResponseCode"].ToString();
            var transactionStatus = query["vnp_TransactionStatus"].ToString();
            var isSuccess = responseCode == "00" && (string.IsNullOrWhiteSpace(transactionStatus) || transactionStatus == "00");

            return Task.FromResult(new PaymentVerificationResult
            {
                IsSuccess = isSuccess,
                // Tới đây chữ ký đã khớp -> dữ liệu đúng là do VNPAY ký và gửi về
                IsAuthentic = true,
                TransactionId = query["vnp_TransactionNo"].ToString(),
                Message = isSuccess ? "Thanh toán VNPAY thành công." : $"Thanh toán VNPAY không thành công. Mã lỗi: {responseCode}"
            });
        }

        /// <summary>
        /// Hoàn tiền qua Merchant API của VNPAY (vnp_Command = "refund").
        /// Bản cũ chỉ `return true` giả lập nên hệ thống báo đã hoàn tiền trong khi tiền vẫn nằm
        /// ở VNPAY - khách không nhận được đồng nào.
        ///
        /// Chữ ký của lệnh refund KHÁC lệnh thanh toán: không phải query string sắp xếp theo tên,
        /// mà là chuỗi các trường nối bằng dấu "|" theo đúng thứ tự VNPAY quy định.
        /// </summary>
        public async Task<bool> RefundAsync(string transactionId, decimal amount, string? providerSessionId = null, DateTime? originalPaidAt = null, bool isFullRefund = false)
        {
            var tmnCode = _configuration["VNPAY_TMN_CODE"] ?? _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VNPAY_HASH_SECRET"] ?? _configuration["VnPay:HashSecret"];
            var apiUrl = _configuration["VnPay:ApiUrl"] ?? "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";

            if (string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret))
                throw new InvalidOperationException("VNPAY chưa được cấu hình TmnCode/HashSecret.");

            if (string.IsNullOrWhiteSpace(providerSessionId))
                throw new InvalidOperationException("Hoàn tiền VNPAY cần mã tham chiếu đơn (vnp_TxnRef).");

            if (amount <= 0)
                throw new InvalidOperationException("Số tiền hoàn phải lớn hơn 0.");

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1")
                ipAddress = "127.0.0.1";

            var requestId = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var version = "2.1.0";
            var command = "refund";
            // 02 = hoàn toàn phần, 03 = hoàn một phần. Gửi sai loại có thể bị VNPAY từ chối,
            // nên phải bám theo việc khách trả hết đơn hay chỉ trả vài món.
            var transactionType = isFullRefund ? "02" : "03";
            var amountStr = ((long)Math.Round(amount * 100, 0)).ToString(CultureInfo.InvariantCulture);
            var transactionDate = (originalPaidAt ?? DateTime.Now).ToString("yyyyMMddHHmmss");
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var createBy = "system";
            var orderInfo = $"Hoan tien don hang {providerSessionId}";
            var transactionNo = transactionId ?? string.Empty;

            // Thứ tự các trường trong chuỗi ký là bắt buộc, sai thứ tự là VNPAY trả mã 97
            var signData = string.Join("|",
                requestId, version, command, tmnCode, transactionType, providerSessionId,
                amountStr, transactionNo, transactionDate, createBy, createDate, ipAddress, orderInfo);

            var secureHash = ComputeHmacSha512(signData, hashSecret);

            var payload = new Dictionary<string, string>
            {
                ["vnp_RequestId"] = requestId,
                ["vnp_Version"] = version,
                ["vnp_Command"] = command,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_TransactionType"] = transactionType,
                ["vnp_TxnRef"] = providerSessionId,
                ["vnp_Amount"] = amountStr,
                ["vnp_TransactionNo"] = transactionNo,
                ["vnp_TransactionDate"] = transactionDate,
                ["vnp_CreateBy"] = createBy,
                ["vnp_CreateDate"] = createDate,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_SecureHash"] = secureHash
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(apiUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"VNPAY từ chối yêu cầu hoàn tiền (HTTP {(int)response.StatusCode}): {body}");

            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var responseCode = doc.RootElement.TryGetProperty("vnp_ResponseCode", out var rc) ? rc.GetString() : null;
            var message = doc.RootElement.TryGetProperty("vnp_Message", out var msg) ? msg.GetString() : body;

            if (responseCode != "00")
                throw new InvalidOperationException($"VNPAY hoàn tiền không thành công (mã {responseCode}): {message}");

            return true;
        }

        private static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> data, bool encode)
        {
            return string.Join("&", data.Select(item =>
            {
                var key = encode ? WebUtility.UrlEncode(item.Key) : item.Key;
                var value = encode ? WebUtility.UrlEncode(item.Value) : item.Value;
                return $"{key}={value}";
            }));
        }

        private static string ComputeHmacSha512(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
