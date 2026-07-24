using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class CampaignAddonProductRule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CampaignId { get; set; }
        [ForeignKey("CampaignId")]
        public virtual PromotionCampaign Campaign { get; set; }

        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }
    }
}
