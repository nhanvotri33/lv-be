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

        public VnPayPaymentProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ProviderName => "vnpay";

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

        public Task<PaymentVerificationResult> VerifySessionAsync(string sessionId)
        {
            var query = _httpContextAccessor.HttpContext?.Request.Query;
            if (query == null || !query.ContainsKey("vnp_SecureHash"))
            {
                return Task.FromResult(new PaymentVerificationResult
                {
                    IsSuccess = false,
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
                TransactionId = query["vnp_TransactionNo"].ToString(),
                Message = isSuccess ? "Thanh toán VNPAY thành công." : $"Thanh toán VNPAY không thành công. Mã lỗi: {responseCode}"
            });
        }

        public Task<bool> RefundAsync(string transactionId, decimal amount)
        {
            // Giả lập hoàn tiền VNPAY thành công
            return Task.FromResult(true);
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
