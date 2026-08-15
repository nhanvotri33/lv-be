// ==========================================================================
// MODULE: UserNotification.cs
// MỤC ĐÍCH: Model lưu thông báo trong ứng dụng (In-App Notification) cho User
// ==========================================================================
using System;
using ECommerce.Models;

namespace ECommerce1.Models
{
    public class UserNotification
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "PriceDrop", "Restock", "System"

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
