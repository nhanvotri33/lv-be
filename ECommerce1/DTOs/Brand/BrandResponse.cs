using System;

namespace ECommerce1.DTOs.Brand
{
    public class BrandResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string BrandCode { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }
}
