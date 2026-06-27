using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	    public class ShippingInfo
    {
        [Key]
        public int Id { get; set; }
        public string RecipientName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        [MaxLength(20)]
        public string WardId { get; set; }
        [ForeignKey("WardId")]
        public virtual Ward Ward { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User User { get; set; }
    }
}
