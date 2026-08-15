// ==========================================================================
// MODULE: OrderStatus.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module OrderStatus
// ==========================================================================
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
