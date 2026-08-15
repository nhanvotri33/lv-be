// ==========================================================================
// MODULE: Wishlist.cs
// MỤC ĐÍCH: Model lưu sản phẩm yêu thích và cài đặt thông báo của User
// ==========================================================================
using System;
using ECommerce.Models;

namespace ECommerce1.Models
{
    public class Wishlist
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public bool NotifyOnPriceDrop { get; set; } = true;
        public bool NotifyOnRestock { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
