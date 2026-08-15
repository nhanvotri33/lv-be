// ==========================================================================
// MODULE: NotificationDTOs.cs
// MỤC ĐÍCH: Data Transfer Objects cho hệ thống Thông báo (UserNotification)
// ==========================================================================
using System;
using System.Collections.Generic;

namespace ECommerce1.DTOs.Notification
{
    public class UserNotificationResponse
    {
        public Guid Id { get; set; }
        public int? ProductId { get; set; }
        public string ProductSlug { get; set; }
        public string ProductImage { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "PriceDrop", "Restock", "System"
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationListResponse
    {
        public int UnreadCount { get; set; }
        public List<UserNotificationResponse> Items { get; set; } = new List<UserNotificationResponse>();
    }
}
