// ==========================================================================
// MODULE: IAhamoveService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module IAhamoveService
// ==========================================================================
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IAhamoveService
    {
        Task<string> GetTokenAsync();
        Task<decimal> EstimateFeeAsync(double destLat, double destLng, string destAddress, string serviceId = "SGN-BIKE");
        Task<AhamoveOrderResponse> CreateOrderAsync(ECommerce.Models.Order order, string serviceId = "SGN-BIKE");
    }

    public class AhamoveOrderResponse
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string SharedLink { get; set; }
        public decimal TotalFee { get; set; }
    }
}
