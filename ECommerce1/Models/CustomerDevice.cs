using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECommerce1.Models;

namespace ECommerce.Models
{
    public class CustomerDevice
    {
        [Key]
        public int Id { get; set; }

        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        [StringLength(100)]
        public string ImeiOrSerial { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductName { get; set; }

        public int? VariantId { get; set; }
        [ForeignKey("VariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        public DateTime PurchaseDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
