// ==========================================================================
// MODULE: Category.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module Category
// ==========================================================================
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
        public string? SpecsTemplate { get; set; }

        // TỰ LIÊN KẾT (SELF-REFERENCING): Thiết lập mối quan hệ cha-con trong cùng một bảng Categories
        // ParentId lưu trữ Id của danh mục cha. Nếu ParentId = null, đây là danh mục gốc (Cấp 1).
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        // Đối tượng danh mục cha tương ứng (Bên "1" của quan hệ)
        public virtual Category ParentCategory { get; set; }

        // Danh sách các danh mục con trực thuộc (Bên "Nhiều" của quan hệ)
        public virtual ICollection<Category> SubCategories { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
