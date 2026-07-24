using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce1.DTOs.PromotionCampaign
{
    public class PromotionCampaignRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; }
        
        public decimal DiscountValue { get; set; }
        
        public bool IsActive { get; set; }
        public int MaxQuantityAllowed { get; set; } = 5;
        public decimal? MaxDiscountAmount { get; set; }

        public List<CampaignRuleDto> MainProductRules { get; set; } = new List<CampaignRuleDto>();
        public List<CampaignRuleDto> AddonProductRules { get; set; } = new List<CampaignRuleDto>();
    }

    public class CampaignRuleDto
    {
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
    }
}
