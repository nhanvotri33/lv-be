// ==========================================================================
// MODULE: IProductService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module IProductService
// ==========================================================================
using ECommerce1.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllAsync(
            int? categoryId = null,
            string? brand = null,
            string? search = null,
            string? sortBy = null,
            string? sortOrder = null,
            bool includeInactive = false);
        Task<ProductResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(ProductRequest request);
        Task UpdateAsync(int id, ProductRequest request);
        Task DeleteAsync(int id);
    }
}
