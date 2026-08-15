// ==========================================================================
// MODULE: WishlistDTOs.cs
// MỤC ĐÍCH: Data Transfer Objects cho tính năng Yêu thích (Wishlist)
// ==========================================================================
using System;

namespace ECommerce1.DTOs.Wishlist
{
    public class WishlistResponse
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSlug { get; set; }
        public string ProductImage { get; set; }
        public decimal BasePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TotalStock { get; set; }
        public bool NotifyOnPriceDrop { get; set; }
        public bool NotifyOnRestock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ToggleWishlistRequest
    {
        public int ProductId { get; set; }
    }

    public class UpdateWishlistNotificationRequest
    {
        public int ProductId { get; set; }
        public bool NotifyOnPriceDrop { get; set; }
        public bool NotifyOnRestock { get; set; }
    }
}
