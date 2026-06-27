using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int TotalStock { get; set; }
        public int ReservedStock { get; set; }
        
        [NotMapped]
        public int AvailableStock => TotalStock - ReservedStock;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public string ImageId { get; set; } 
        
        // JSON string storing dynamic attributes like {"Color": "Black", "RAM": "8GB"}
        public string Attributes { get; set; }
        
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }
}