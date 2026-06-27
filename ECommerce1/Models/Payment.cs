using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User User { get; set; }

        public string Provider { get; set; } // e.g. "Stripe", "VNPay"
        public string ProviderSessionId { get; set; }
        public string ProviderTransactionId { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; } // pending, succeeded, failed

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
