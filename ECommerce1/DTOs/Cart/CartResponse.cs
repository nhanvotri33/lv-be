using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce1.DTOs.Cart
{
    public class CartResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        
        public decimal TotalPrice => Items.Sum(i => i.SubTotal);
    }
}
