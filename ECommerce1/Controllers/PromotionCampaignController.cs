// ==========================================================================
// MODULE: PromotionCampaignController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module PromotionCampaignController
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.PromotionCampaign;
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
    public class PromotionCampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionCampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= ADMIN APIs =================

        // [API Endpoint GET]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetAllCampaigns` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllCampaigns()
        {
            var campaigns = await _context.PromotionCampaigns
                .Include(c => c.MainProductRules)
                    .ThenInclude(r => r.Product)
                .Include(c => c.MainProductRules)
                    .ThenInclude(r => r.Category)
                .Include(c => c.MainProductRules)
                    .ThenInclude(r => r.Brand)
                .Include(c => c.AddonProductRules)
                    .ThenInclude(r => r.Product)
                .Include(c => c.AddonProductRules)
                    .ThenInclude(r => r.Category)
                .Include(c => c.AddonProductRules)
                    .ThenInclude(r => r.Brand)
                .OrderByDescending(c => c.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

            var response = campaigns.Select(MapToResponse).ToList();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }

        // [API Endpoint GET [Route: `{id}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetCampaign` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetCampaign(int id)
        {
            var campaign = await _context.PromotionCampaigns
                .Include(c => c.MainProductRules)
                    .ThenInclude(r => r.Product)
                .Include(c => c.MainProductRules)
                    .ThenInclude(r => r.Category)
                .Include(c => c.MainProductRules)
                    .ThenInclude(r => r.Brand)
                .Include(c => c.AddonProductRules)
                    .ThenInclude(r => r.Product)
                .Include(c => c.AddonProductRules)
                    .ThenInclude(r => r.Category)
                .Include(c => c.AddonProductRules)
                    .ThenInclude(r => r.Brand)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy chiến dịch.");

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(MapToResponse(campaign));
        }

        // [API Endpoint POST]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPost]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `CreateCampaign` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CreateCampaign([FromBody] PromotionCampaignRequest request)
        {
            if (!ModelState.IsValid)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest(ModelState);

            var campaign = new PromotionCampaign
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                IsActive = request.IsActive,
                MaxQuantityAllowed = request.MaxQuantityAllowed,
                MaxDiscountAmount = request.MaxDiscountAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (request.MainProductRules != null && request.MainProductRules.Any())
            {
                campaign.MainProductRules = request.MainProductRules.Select(r => new CampaignMainProductRule
                {
                    ProductId = r.ProductId,
                    CategoryId = r.CategoryId,
                    BrandId = r.BrandId
                }).ToList();
            }

            if (request.AddonProductRules != null && request.AddonProductRules.Any())
            {
                campaign.AddonProductRules = request.AddonProductRules.Select(r => new CampaignAddonProductRule
                {
                    ProductId = r.ProductId,
                    CategoryId = r.CategoryId,
                    BrandId = r.BrandId
                }).ToList();
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.PromotionCampaigns.Add(campaign);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { Message = "Tạo chiến dịch thành công.", Id = campaign.Id });
        }

        // [API Endpoint PUT [Route: `{id}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `UpdateCampaign` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] PromotionCampaignRequest request)
        {
            var campaign = await _context.PromotionCampaigns
                .Include(c => c.MainProductRules)
                .Include(c => c.AddonProductRules)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy chiến dịch.");

            campaign.Name = request.Name;
            campaign.Description = request.Description;
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            campaign.DiscountType = request.DiscountType;
            campaign.DiscountValue = request.DiscountValue;
            campaign.IsActive = request.IsActive;
            campaign.MaxQuantityAllowed = request.MaxQuantityAllowed;
            campaign.MaxDiscountAmount = request.MaxDiscountAmount;
            campaign.UpdatedAt = DateTime.UtcNow;

            // Cập nhật rules
            _context.CampaignMainProductRules.RemoveRange(campaign.MainProductRules);
            if (request.MainProductRules != null)
            {
                campaign.MainProductRules = request.MainProductRules.Select(r => new CampaignMainProductRule
                {
                    ProductId = r.ProductId,
                    CategoryId = r.CategoryId,
                    BrandId = r.BrandId
                }).ToList();
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.CampaignAddonProductRules.RemoveRange(campaign.AddonProductRules);
            if (request.AddonProductRules != null)
            {
                campaign.AddonProductRules = request.AddonProductRules.Select(r => new CampaignAddonProductRule
                {
                    ProductId = r.ProductId,
                    CategoryId = r.CategoryId,
                    BrandId = r.BrandId
                }).ToList();
            }

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cập nhật chiến dịch thành công.");
        }

        // [API Endpoint DELETE [Route: `{id}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `DeleteCampaign` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var campaign = await _context.PromotionCampaigns.FindAsync(id);
            if (campaign == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy chiến dịch.");

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.PromotionCampaigns.Remove(campaign);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Xóa chiến dịch thành công.");
        }

        // ================= CLIENT APIs =================

        // [API Endpoint GET [Route: `product/{productId}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        // [Hàm thực thi nghiệp vụ]: `GetCampaignsForProduct` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetCampaignsForProduct(int productId)
        {
            // 1. Get product info
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Sản phẩm không tồn tại.");

            // 2. Lọc các chiến dịch còn hiệu lực và IsActive
            var now = DateTime.UtcNow;
            var activeCampaigns = await _context.PromotionCampaigns
                .AsNoTracking()
                .Include(c => c.MainProductRules)
                .Include(c => c.AddonProductRules)
                .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
                .ToListAsync();

            var applicableCampaigns = new List<object>();

            var ancestorCatIds = await GetAncestorCategoryIds(product.CategoryId);

            // 3. Tìm các campaign thỏa mãn (Logic AND trong từng dòng quy tắc, OR giữa các dòng quy tắc)
            foreach (var campaign in activeCampaigns)
            {
                bool isApplicable = false;
                if (campaign.MainProductRules == null || !campaign.MainProductRules.Any())
                {
                    // Nếu không cấu hình MainProductRule, áp dụng cho tất cả sản phẩm
                    isApplicable = true;
                }
                else
                {
                    foreach (var rule in campaign.MainProductRules)
                    {
                        bool matchesRule = true;

                        // Nếu có ProductId mà không khớp -> Fail
                        if (rule.ProductId.HasValue && rule.ProductId.Value != productId)
                            matchesRule = false;

                        // Nếu có CategoryId mà không khớp -> Fail
                        if (matchesRule && rule.CategoryId.HasValue && !ancestorCatIds.Contains(rule.CategoryId.Value))
                            matchesRule = false;

                        // Nếu có BrandId mà không khớp -> Fail
                        if (matchesRule && rule.BrandId.HasValue && rule.BrandId.Value != product.BrandId)
                            matchesRule = false;

                        // Nếu thỏa mãn tất cả điều kiện non-null của dòng quy tắc này
                        if (matchesRule)
                        {
                            isApplicable = true;
                            break; // Thỏa mãn 1 dòng quy tắc (OR giữa các dòng)
                        }
                    }
                }

                if (isApplicable)
                {
                    // 4. Resolve addon products for this campaign (AND trong dòng, OR giữa các dòng)
                    var addonProductIds = new HashSet<int>();
                    var explicitProductIds = new HashSet<int>();

                    foreach (var rule in campaign.AddonProductRules)
                    {
                        // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                        var ruleQuery = _context.Products.Where(p => p.IsActive);
                        bool hasCriteria = false;

                        if (rule.ProductId.HasValue)
                        {
                            ruleQuery = ruleQuery.Where(p => p.Id == rule.ProductId.Value);
                            explicitProductIds.Add(rule.ProductId.Value);
                            hasCriteria = true;
                        }
                        if (rule.CategoryId.HasValue)
                        {
                            var descendantCatIds = await GetDescendantCategoryIds(rule.CategoryId.Value);
                            ruleQuery = ruleQuery.Where(p => descendantCatIds.Contains(p.CategoryId));
                            hasCriteria = true;
                        }
                        if (rule.BrandId.HasValue)
                        {
                            ruleQuery = ruleQuery.Where(p => p.BrandId == rule.BrandId.Value);
                            hasCriteria = true;
                        }

                        if (hasCriteria)
                        {
                            var matchingIds = await ruleQuery.Select(p => p.Id).ToListAsync();
                            foreach (var id in matchingIds) addonProductIds.Add(id);
                        }
                    }

                    // Remove main product from addon list just in case
                    addonProductIds.Remove(productId);

                    // Lấy chi tiết các sản phẩm phụ
                    var addonProductsDb = await _context.Products
                        .AsNoTracking()
                        .Where(p => addonProductIds.Contains(p.Id) && p.IsActive)
                        .Include(p => p.ProductVariants)
                        .ToListAsync();

                    var addonProducts = addonProductsDb.Select(p => new
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Slug = p.Slug,
                            ThumbnailImage = p.ThumbnailImage,
                            BasePrice = p.BasePrice,
                            IsExplicitlyAdded = explicitProductIds.Contains(p.Id),
                            Variants = p.ProductVariants.Select(pv => new
                            {
                                Id = pv.Id,
                                Name = pv.Name,
                                Price = pv.Price,
                                AvailableStock = pv.AvailableStock
                            }).ToList()
                        })
                        .ToList();

                    if (addonProducts.Any())
                    {
                        applicableCampaigns.Add(new
                        {
                            Campaign = new
                            {
                                Id = campaign.Id,
                                Name = campaign.Name,
                                Description = campaign.Description,
                                DiscountType = campaign.DiscountType,
                                DiscountValue = campaign.DiscountValue,
                                MaxQuantityAllowed = campaign.MaxQuantityAllowed,
                                MaxDiscountAmount = campaign.MaxDiscountAmount
                            },
                            AddonProducts = addonProducts
                        });
                    }
                }
            }

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(applicableCampaigns);
        }

        /// <summary>
        /// Lấy tất cả ID danh mục con (bao gồm cả chính nó) theo BFS.
        /// Điều này cho phép quy tắc "Danh mục cha" tự động bao gồm các sản phẩm trong danh mục con.
        /// </summary>
        private async Task<HashSet<int>> GetDescendantCategoryIds(int rootCategoryId)
        {
            var result = new HashSet<int> { rootCategoryId };
            var queue = new Queue<int>();
            queue.Enqueue(rootCategoryId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var children = await _context.Categories
                    .Where(c => c.ParentId == current)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var childId in children)
                {
                    if (result.Add(childId)) // Add returns false if already exists
                        queue.Enqueue(childId);
                }
            }

            return result;
        }

        /// <summary>
        /// Lấy tất cả ID tổ tiên (bao gồm chính nó) của một danh mục.
        /// Dùng để kiểm tra sản phẩm có thuộc quy tắc CategoryId của campaign không.
        /// </summary>
        private async Task<HashSet<int>> GetAncestorCategoryIds(int categoryId)
        {
            var result = new HashSet<int> { categoryId };
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var current = await _context.Categories.FindAsync(categoryId);

            while (current?.ParentId != null)
            {
                result.Add(current.ParentId.Value);
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                current = await _context.Categories.FindAsync(current.ParentId.Value);
            }

            return result;
        }

        private PromotionCampaignResponse MapToResponse(PromotionCampaign campaign)
        {
            return new PromotionCampaignResponse
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                DiscountType = campaign.DiscountType,
                DiscountValue = campaign.DiscountValue,
                IsActive = campaign.IsActive,
                MaxQuantityAllowed = campaign.MaxQuantityAllowed,
                MainProductRules = campaign.MainProductRules?.Select(r => new CampaignRuleResponseDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category?.Name,
                    BrandId = r.BrandId,
                    BrandName = r.Brand?.Name
                }).ToList() ?? new List<CampaignRuleResponseDto>(),
                AddonProductRules = campaign.AddonProductRules?.Select(r => new CampaignRuleResponseDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category?.Name,
                    BrandId = r.BrandId,
                    BrandName = r.Brand?.Name
                }).ToList() ?? new List<CampaignRuleResponseDto>()
            };
        }
    }
}
