using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [MaxLength(20)]
        public string CategoryCode { get; set; }

        public string Slug { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }



        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
