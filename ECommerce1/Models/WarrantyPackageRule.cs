using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class WarrantyPackageRule
    {
        [Key]
        public int Id { get; set; }

        public int WarrantyId { get; set; }
        [ForeignKey("WarrantyId")]
        public virtual Warranty Warranty { get; set; }

        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }

        public decimal MinPrice { get; set; } = 0;

        public decimal? MaxPrice { get; set; }
    }
}
