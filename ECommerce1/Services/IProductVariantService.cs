using ECommerce1.DTOs.ProductVariant;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariantResponse>> GetAllAsync(int? productId);
        Task<ProductVariantResponse> GetByIdAsync(int id);
        Task CreateAsync(ProductVariantRequest request);
        Task CreateBatchAsync(List<ProductVariantRequest> requests);
        Task UpdateAsync(int id, ProductVariantRequest request);
        Task SyncAsync(int productId, List<ProductVariantRequest> requests);
        Task DeleteAsync(int id);
    }
}
