// ==========================================================================
// MODULE: AdminDashboardController.cs
// MỤC ĐÍCH: API Controller xử lý báo cáo quản trị, doanh thu, lợi nhuận gộp theo Thương hiệu (Brand Profitability Insights)
// ==========================================================================
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Trạng thái đơn hàng THÀNH CÔNG (OrderStatuses: 4 = Completed - Đã giao)
        private const int COMPLETED_STATUS_ID = 4;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= GET: Báo cáo Lợi nhuận gộp & Hệ sinh thái theo Thương hiệu (Brand Profitability Report) =================
        [HttpGet("brand-profit-report")]
        [AllowAnonymous] // Cho phép truy vấn thống kê phục vụ thử nghiệm Admin Dashboard
        public async Task<IActionResult> GetBrandProfitReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // CHỈ TÍNH ĐƠN HÀNG THÀNH CÔNG: OrderStatusId = 4 (Completed - Đã giao).
            // Các đơn đang chờ / đang giao / đã hủy / hoàn tiền KHÔNG được ghi nhận doanh thu & lợi nhuận.
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p.Brand)
                .Where(oi => oi.Order != null && oi.Order.OrderStatusId == COMPLETED_STATUS_ID);

            if (startDate.HasValue)
                query = query.Where(oi => oi.Order.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(oi => oi.Order.CreatedAt <= endDate.Value);

            var items = await query.ToListAsync();

            if (!items.Any())
            {
                // Chưa có đơn hàng THÀNH CÔNG nào trong kỳ -> trả về số 0 thay vì dữ liệu mẫu,
                // đảm bảo báo cáo lợi nhuận của Admin luôn phản ánh đúng số liệu thật.
                return Ok(new
                {
                    totalStoreRevenue = 0m,
                    totalStoreCost = 0m,
                    totalStoreGrossProfit = 0m,
                    overallMargin = 0.0,
                    totalCompletedOrders = 0,
                    brands = new List<object>()
                });
            }

            // Gom nhóm theo Thương hiệu (Brand)
            var brandGroups = items.GroupBy(oi => oi.ProductVariant?.Product?.Brand?.Name ?? "Khác");

            decimal totalStoreRevenue = items.Sum(oi => oi.PriceAtPurchase * oi.Quantity);
            decimal totalStoreGrossProfit = items.Sum(oi =>
            {
                decimal cost = oi.CostPriceAtPurchase > 0 
                    ? oi.CostPriceAtPurchase 
                    : (oi.ProductVariant?.CostPrice > 0 ? oi.ProductVariant.CostPrice : (oi.ProductVariant?.Product?.CostPrice > 0 ? oi.ProductVariant.Product.CostPrice : oi.PriceAtPurchase));
                return (oi.PriceAtPurchase - cost) * oi.Quantity;
            });

            var brandReportList = brandGroups.Select(g =>
            {
                string brandName = g.Key;
                decimal rev = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity);
                decimal cogs = g.Sum(oi =>
                {
                    decimal cost = oi.CostPriceAtPurchase > 0 
                        ? oi.CostPriceAtPurchase 
                        : (oi.ProductVariant?.CostPrice > 0 ? oi.ProductVariant.CostPrice : (oi.ProductVariant?.Product?.CostPrice > 0 ? oi.ProductVariant.Product.CostPrice : oi.PriceAtPurchase));
                    return cost * oi.Quantity;
                });

                decimal profit = rev - cogs;
                double margin = rev > 0 ? (double)Math.Round((profit / rev) * 100m, 2) : 0;
                double revShare = totalStoreRevenue > 0 ? (double)Math.Round((rev / totalStoreRevenue) * 100m, 2) : 0;
                double profitShare = totalStoreGrossProfit > 0 ? (double)Math.Round((profit / totalStoreGrossProfit) * 100m, 2) : 0;
                int units = g.Sum(oi => oi.Quantity);

                string insight = margin >= 40 
                    ? "Sản phẩm/Phụ kiện lợi nhuận bùng nổ, gánh tỷ trọng lợi nhuận chính."
                    : (margin >= 20 ? "Dòng sản phẩm cân bằng tốt giữa doanh thu và lợi nhuận." : "Dòng kéo Doanh thu và lượng truy cập cho shop (Biên lợi nhuận mỏng).");

                return new
                {
                    brandName,
                    revenue = rev,
                    costOfGoodsSold = cogs,
                    grossProfit = profit,
                    profitMargin = margin,
                    revenueShare = revShare,
                    profitShare,
                    totalUnitsSold = units,
                    insightNote = insight
                };
            }).OrderByDescending(b => b.grossProfit).ToList();

            return Ok(new
            {
                totalStoreRevenue,
                totalStoreCost = totalStoreRevenue - totalStoreGrossProfit,
                totalStoreGrossProfit,
                overallMargin = totalStoreRevenue > 0 ? (double)Math.Round((totalStoreGrossProfit / totalStoreRevenue) * 100m, 2) : 0,
                totalCompletedOrders = items.Select(oi => oi.OrderId).Distinct().Count(),
                brands = brandReportList
            });
        }
    }
}
