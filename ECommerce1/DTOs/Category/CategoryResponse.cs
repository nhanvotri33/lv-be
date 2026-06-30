using System;

namespace ECommerce1.DTOs.Category
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string CategoryCode { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public int SubCategoriesCount { get; set; }
        public int ProductsCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? SpecsTemplate { get; set; }
    }
}