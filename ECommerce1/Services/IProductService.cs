using ECommerce1.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllAsync(bool includeInactive = false);
        Task<ProductResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(ProductRequest request);
        Task UpdateAsync(int id, ProductRequest request);
        Task DeleteAsync(int id);
    }
}
