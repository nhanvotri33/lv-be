using ECommerce1.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid userId);
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
        Task<object> CheckoutAsync(Guid userId, CheckoutRequest request);
        Task CancelOrderAsync(int id, Guid? userId, string? phoneNumber);
        Task UpdateOrderStatusAsync(int id, int newStatusId);
        Task<OrderResponse> TrackOrderAsync(int id, string phoneNumber);
    }
}
