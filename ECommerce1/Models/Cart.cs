using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerce.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
