using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce1.DTOs.Combo
{
    public class ProductComboRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public List<ProductComboItemRequest> Items { get; set; } = new List<ProductComboItemRequest>();
    }

    public class ProductComboItemRequest
    {
        public int ProductId { get; set; }

        public bool IsMain { get; set; }

        public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "FixedAmount"

        public decimal DiscountValue { get; set; }
    }
}
