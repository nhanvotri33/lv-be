using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [MaxLength(20)]
        public string ProductCode { get; set; }

        public string Slug { get; set; }
        public string Description { get; set; }
        public string? Specs { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int TotalStock { get; set; }
        public int ReservedStock { get; set; }
        
        [NotMapped]
        public int AvailableStock => TotalStock - ReservedStock;

        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }



        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        public string ThumbnailImage { get; set; }
        public string MainImage { get; set; }
        public string Images { get; set; } // Có thể lưu chuỗi JSON hoặc mảng tùy database
        public string? VideoUrl { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
