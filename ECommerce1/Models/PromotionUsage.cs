using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ECommerce.Models
{
    public class PromotionUsage
    {
        [Key]
        public int Id { get; set; }
        public DateTime UsedAt { get; set; }

        public int? PromotionId { get; set; }
        [ForeignKey("PromotionId")]
        public virtual Promotion Promotion { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ECommerce1.Models.User User { get; set; }
    }
}
