using ECommerce.Models;
using Microsoft.Extensions.Configuration;
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
            Stripe.StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public string ProviderName => "stripe";

        public async Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            foreach (var item in order.OrderItems)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.PriceAtPurchase),
                        Currency = "vnd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductVariant?.Product?.Name ?? "Sản phẩm",
                        },
                    },
                    Quantity = item.Quantity,
                });
            }

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Id;
        }

        public async Task<PaymentVerificationResult> VerifySessionAsync(string sessionId)
        {
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
    }
}
