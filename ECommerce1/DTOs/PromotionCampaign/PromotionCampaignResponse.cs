using System;
using System.Collections.Generic;

namespace ECommerce1.DTOs.PromotionCampaign
{
    public class PromotionCampaignResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsActive { get; set; }
        public int MaxQuantityAllowed { get; set; }
        public decimal? MaxDiscountAmount { get; set; }

        public List<CampaignRuleResponseDto> MainProductRules { get; set; } = new List<CampaignRuleResponseDto>();
        public List<CampaignRuleResponseDto> AddonProductRules { get; set; } = new List<CampaignRuleResponseDto>();
    }

    public class CampaignRuleResponseDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
    }
}
