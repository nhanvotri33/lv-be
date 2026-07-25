using ECommerce.Models;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce1.Services.Payment
{
    public class StripePaymentProvider : IPaymentProvider
    {
        private readonly IConfiguration _configuration;

        public StripePaymentProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            var apiKey = _configuration["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                Stripe.StripeConfiguration.ApiKey = apiKey;
            }
        }

        public string ProviderName => "stripe";

        public async Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
        {
            var apiKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // Chế độ giả lập khi chưa cấu hình Stripe API Key
                return "mock_stripe_session_" + Guid.NewGuid().ToString().Substring(0, 8);
            }

            // Trích xuất danh sách sản phẩm hiển thị trong mô tả của Stripe Checkout
            var productNamesList = order.OrderItems != null 
                ? string.Join(", ", order.OrderItems.Select(i => $"{i.ProductVariant?.Product?.Name ?? "Sản phẩm"} (x{i.Quantity})"))
                : "Thanh toán đơn hàng";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)order.TotalPrice,
                            Currency = "vnd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Thanh toán đơn hàng #PS{order.Id}",
                                Description = productNamesList.Length > 250 ? productNamesList.Substring(0, 247) + "..." : productNamesList
                            },
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Id;
        }

        public async Task<PaymentVerificationResult> VerifySessionAsync(string sessionId)
        {
            if (sessionId.StartsWith("mock_stripe_session_"))
            {
                return new PaymentVerificationResult
                {
                    IsSuccess = true,
                    TransactionId = "mock_stripe_trans_" + Guid.NewGuid().ToString().Substring(0, 8),
                    Message = "Thanh toán Stripe giả lập thành công (Chưa cấu hình API Key)"
                };
            }

            var service = new SessionService();
            Session session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                return new PaymentVerificationResult
                {
                    IsSuccess = true,
                    TransactionId = session.PaymentIntentId,
                    Message = "Thanh toán Stripe thành công"
                };
            }

            return new PaymentVerificationResult
            {
                IsSuccess = false,
                TransactionId = null,
                Message = "Thanh toán chưa hoàn tất"
            };
        }

        public async Task<bool> RefundAsync(string transactionId, decimal amount)
        {
            if (string.IsNullOrEmpty(transactionId))
                throw new ArgumentException("Mã giao dịch Stripe không hợp lệ.");

            // Lấy thông tin PaymentIntent thực tế để lấy đúng số tiền đã charge trên Stripe (tránh lệch số do phí ship/giảm giá chưa đồng bộ của các đơn hàng cũ)
            var piService = new PaymentIntentService();
            var paymentIntent = await piService.GetAsync(transactionId);
            long actualAmount = paymentIntent.Amount;

            var options = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Amount = actualAmount
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            return refund.Status == "succeeded" || refund.Status == "pending";
        }
    }
}
