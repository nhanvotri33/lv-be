using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string BrandCode { get; set; }
        
        [MaxLength(255)]
        public string Slug { get; set; }
        
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Product> Products { get; set; }
    }
}
