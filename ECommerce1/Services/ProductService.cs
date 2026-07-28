using ECommerce.Models;
using ECommerce1.DTOs.Product;
using ECommerce1.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ProductService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        private async Task<HashSet<int>> GetValidCategoryIdsAsync()
        {
            var allCats = await _context.Categories.ToListAsync();
            var validIds = new HashSet<int>();
            
            var level1 = allCats.Where(c => c.ParentId == null && c.IsActive != false).ToList();
            foreach (var c1 in level1)
            {
                validIds.Add(c1.Id);
                
                var level2 = allCats.Where(c => c.ParentId == c1.Id && c.IsActive != false).ToList();
                foreach (var c2 in level2)
                {
                    validIds.Add(c2.Id);
                    
                    var level3 = allCats.Where(c => c.ParentId == c2.Id && c.IsActive != false).ToList();
                    foreach (var c3 in level3)
                    {
                        validIds.Add(c3.Id);
                    }
                }
            }
            
            return validIds;
        }

        private async Task<HashSet<int>> GetCategoryDescendantsAsync(int parentId)
        {
            var allCats = await _context.Categories.ToListAsync();
            var result = new HashSet<int> { parentId };
            
            void AddChildren(int id)
            {
                var children = allCats.Where(c => c.ParentId == id).ToList();
                foreach (var child in children)
                {
                    if (result.Add(child.Id))
                    {
                        AddChildren(child.Id);
                    }
                }
            }
            
            AddChildren(parentId);
            return result;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm kèm lọc theo danh mục, từ khóa và sắp xếp động ở cấp độ Database
        /// </summary>
        public async Task<IEnumerable<ProductResponse>> GetAllAsync(
            int? categoryId = null,
            string? brand = null,
            string? search = null,
            string? sortBy = null,
            string? sortOrder = null,
            bool includeInactive = false)
        {
            var validCategoryIds = await GetValidCategoryIdsAsync();

            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                    .ThenInclude(c => c.ParentCategory)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive != false && 
                                         validCategoryIds.Contains(p.CategoryId) &&
                                         (p.BrandId == null || p.Brand.IsActive != false));
            }

            // 1. LỌC THEO NGỮ CẢNH DANH MỤC (Category Context): Lấy cả danh mục con để tránh lẫn lộn ngành hàng
            if (categoryId.HasValue)
            {
                var categoryBranchIds = await GetCategoryDescendantsAsync(categoryId.Value);
                query = query.Where(p => categoryBranchIds.Contains(p.CategoryId));
            }

            // 1.5 Lọc theo Thương hiệu (Brand)
            if (!string.IsNullOrWhiteSpace(brand))
            {
                var bName = brand.ToLower().Trim();
                query = query.Where(p => p.Brand != null && p.Brand.Name.ToLower() == bName);
            }

            // 2. Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(p => p.Name.ToLower().Contains(term) || 
                                         (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            // 3. Sắp xếp động dưới Database (TGDD Style)
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                bool isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                switch (sortBy.ToLower())
                {
                    case "price":
                        query = isDesc ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice);
                        break;
                    case "featured":
                        // Sắp xếp nổi bật: Ưu tiên các sản phẩm được Admin tích chọn "IsFeatured" (IsFeatured = true) lên đầu, 
                        // sau đó sắp xếp theo ID giảm dần để đưa sản phẩm mới nhất lên.
                        query = isDesc 
                            ? query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Id)
                            : query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Id);
                        break;
                    case "newest":
                        query = isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id);
                        break;
                    case "discount":
                        query = isDesc 
                            ? query.OrderByDescending(p => p.OriginalPrice - p.BasePrice)
                            : query.OrderBy(p => p.OriginalPrice - p.BasePrice);
                        break;
                    case "best_seller":
                        // Sắp xếp bán chạy: Đếm tổng số lượng đánh giá (Reviews) không bị ẩn của sản phẩm. 
                        // Sản phẩm nào được khách mua và đánh giá nhiều nhất sẽ tự động được xếp lên đầu.
                        query = isDesc 
                            ? query.OrderByDescending(p => p.Reviews.Count(r => !r.IsHidden))
                            : query.OrderBy(p => p.Reviews.Count(r => !r.IsHidden));
                        break;
                }
            }

            var productsList = await query.ToListAsync();

            return productsList.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ProductCode = p.ProductCode,
                Description = p.Description,
                Specs = p.Specs,
                BasePrice = p.BasePrice,
                OriginalPrice = p.OriginalPrice,
                TotalStock = p.TotalStock,
                ReservedStock = p.ReservedStock,
                AvailableStock = p.AvailableStock,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                ThumbnailImage = p.ThumbnailImage,
                MainImage = p.MainImage,
                Images = p.Images,
                VideoUrl = p.VideoUrl,
                IsAvailable = p.IsActive && validCategoryIds.Contains(p.CategoryId) && (p.BrandId == null || (p.Brand != null && p.Brand.IsActive != false)),
                BrandIsActive = p.Brand != null ? (bool?)p.Brand.IsActive : null,
                AverageRating = p.Reviews != null && p.Reviews.Any(r => !r.IsHidden) ? p.Reviews.Where(r => !r.IsHidden).Average(r => r.Rating) : 5.0,
                ReviewCount = p.Reviews != null ? p.Reviews.Count(r => !r.IsHidden) : 0,
                IsAccessory = CheckIsAccessory(p)
            }).ToList();
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                    .ThenInclude(c => c.ParentCategory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

            var validCategoryIds = await GetValidCategoryIdsAsync();
            bool isAvailable = product.IsActive && validCategoryIds.Contains(product.CategoryId) && (product.BrandId == null || (product.Brand != null && product.Brand.IsActive != false));

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                ProductCode = product.ProductCode,
                Description = product.Description,
                Specs = product.Specs,
                BasePrice = product.BasePrice,
                OriginalPrice = product.OriginalPrice,
                TotalStock = product.TotalStock,
                ReservedStock = product.ReservedStock,
                AvailableStock = product.AvailableStock,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                BrandName = product.Brand != null ? product.Brand.Name : null,
                ThumbnailImage = product.ThumbnailImage,
                MainImage = product.MainImage,
                Images = product.Images,
                VideoUrl = product.VideoUrl,
                IsAvailable = isAvailable,
                BrandIsActive = product.Brand != null ? (bool?)product.Brand.IsActive : null,
                AverageRating = product.Reviews != null && product.Reviews.Any(r => !r.IsHidden) ? product.Reviews.Where(r => !r.IsHidden).Average(r => r.Rating) : 5.0,
                ReviewCount = product.Reviews != null ? product.Reviews.Count(r => !r.IsHidden) : 0,
                IsAccessory = CheckIsAccessory(product)
            };
        }

        public async Task<int> CreateAsync(ProductRequest request)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
                throw new ArgumentException("Category không tồn tại.");

            // RÀNG BUỘC GIÁ (SERVER-SIDE): Giá bán không được lớn hơn giá gốc (giá niêm yết trước khi giảm)
            if (request.OriginalPrice.HasValue && request.BasePrice > request.OriginalPrice.Value)
                throw new ArgumentException("Giá bán không được lớn hơn giá gốc.");

            // =========================================================================
            // [XỬ LÝ MÃ SẢN PHẨM - BACK-END]
            // - Tự động sinh mã sản phẩm (ProductCode) từ tên nếu Admin bỏ trống khi tạo mới.
            // - Kiểm tra tính duy nhất (Uniqueness Constraint) để tránh trùng mã sản phẩm.
            // =========================================================================
            var productCode = request.ProductCode;
            if (string.IsNullOrWhiteSpace(productCode))
            {
                productCode = CodeGeneratorHelper.GenerateProductCode(request.Name, 20);
            }

            if (await _context.Products.AnyAsync(p => p.ProductCode == productCode))
                throw new ArgumentException("Mã này đã tồn tại.");

            var newProduct = new Product
            {
                Name = request.Name,
                Slug = request.Slug,
                ProductCode = productCode,
                Description = request.Description,
                Specs = request.Specs,
                BasePrice = request.BasePrice,
                OriginalPrice = request.OriginalPrice,
                TotalStock = request.TotalStock,
                ReservedStock = 0,
                IsActive = request.IsActive,
                IsFeatured = request.IsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                ThumbnailImage = request.ThumbnailImage,
                MainImage = request.MainImage,
                Images = request.Images,
                VideoUrl = request.VideoUrl
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            // Tự động sinh Phiếu nhập kho ban đầu (Xử lý ngầm) nếu tồn kho > 0
            if (newProduct.TotalStock > 0)
            {
                // Kiểm tra/Tạo biến thể mặc định để gắn transaction kho
                var defaultVariant = new ProductVariant
                {
                    ProductId = newProduct.Id,
                    Name = "Mặc định",
                    Sku = $"{newProduct.ProductCode}-STD",
                    Price = newProduct.BasePrice,
                    TotalStock = newProduct.TotalStock,
                    ReservedStock = 0,
                    Attributes = "{\"Màu sắc\": \"Mặc định\"}",
                    ImageId = newProduct.ThumbnailImage ?? "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ProductVariants.Add(defaultVariant);
                await _context.SaveChangesAsync();

                // Tạo bản ghi Phiếu nhập kho ban đầu
                var initTx = new InventoryTransaction
                {
                    VariantId = defaultVariant.Id,
                    QuantityChanged = newProduct.TotalStock,
                    TransactionType = "IMPORT",
                    Price = newProduct.BasePrice,
                    Note = "Khởi tạo tồn kho ban đầu",
                    IsReverted = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.InventoryTransactions.Add(initTx);
                await _context.SaveChangesAsync();

                // Lưu vết AuditLog
                var auditLog = new AuditLog
                {
                    Action = "IMPORT",
                    TargetTable = "InventoryTransactions",
                    TargetId = initTx.Id.ToString(),
                    NewValues = $"Khởi tạo tồn kho ban đầu: +{newProduct.TotalStock} sản phẩm cho '{newProduct.Name}'",
                    Timestamp = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }

            return newProduct.Id;
        }

        public async Task UpdateAsync(int id, ProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
                throw new ArgumentException("Category không tồn tại.");

            // RÀNG BUỘC GIÁ (SERVER-SIDE): Giá bán không được lớn hơn giá gốc (giá niêm yết trước khi giảm)
            if (request.OriginalPrice.HasValue && request.BasePrice > request.OriginalPrice.Value)
                throw new ArgumentException("Giá bán không được lớn hơn giá gốc.");

            var productCode = request.ProductCode;
            if (string.IsNullOrWhiteSpace(productCode))
            {
                productCode = CodeGeneratorHelper.GenerateProductCode(request.Name, 20);
            }

            if (await _context.Products.AnyAsync(p => p.ProductCode == productCode && p.Id != id))
                throw new ArgumentException("Mã này đã tồn tại.");

            if (product.ThumbnailImage != request.ThumbnailImage)
            {
                _fileService.DeleteImage(product.ThumbnailImage);
            }
            if (product.MainImage != request.MainImage)
            {
                _fileService.DeleteImage(product.MainImage);
            }

            // Diff gallery images
            var oldImages = ParseImageUrls(product.Images);
            var newImages = ParseImageUrls(request.Images);
            var deletedImages = oldImages.Except(newImages, StringComparer.OrdinalIgnoreCase);
            foreach (var imgUrl in deletedImages)
            {
                _fileService.DeleteImage(imgUrl);
            }

            product.Name = request.Name;
            product.Slug = request.Slug;
            product.ProductCode = productCode;
            product.Description = request.Description;
            product.Specs = request.Specs;
            product.BasePrice = request.BasePrice;
            product.OriginalPrice = request.OriginalPrice;
            product.TotalStock = request.TotalStock;
            if (product.TotalStock - product.ReservedStock <= 0)
            {
                product.IsActive = false;
            }
            else
            {
                product.IsActive = request.IsActive;
            }
            product.IsFeatured = request.IsFeatured;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.ThumbnailImage = request.ThumbnailImage;
            product.MainImage = request.MainImage;
            product.Images = request.Images;
            product.VideoUrl = request.VideoUrl;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private List<string> ParseImageUrls(string imagesStr)
        {
            if (string.IsNullOrEmpty(imagesStr)) return new List<string>();
            
            imagesStr = imagesStr.Trim();
            if (imagesStr.StartsWith("[") && imagesStr.EndsWith("]"))
            {
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<string>>(imagesStr) ?? new List<string>();
                }
                catch
                {
                    // Fallback
                }
            }
            
            return imagesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToList();
        }

        private bool CheckIsAccessory(Product p)
        {
            if (p == null) return false;
            
            var nameLower = (p.Name ?? "").ToLower();
            if (nameLower.Contains("tai nghe") || nameLower.Contains("sạc") || 
                nameLower.Contains("ốp") || nameLower.Contains("kính") || nameLower.Contains("cáp"))
            {
                return true;
            }

            if (p.Category != null)
            {
                var catName = (p.Category.Name ?? "").ToLower();
                if (catName.Contains("phụ kiện") || catName.Contains("tai nghe") || 
                    catName.Contains("cáp") || catName.Contains("sạc") || 
                    catName.Contains("ốp") || catName.Contains("kính"))
                {
                    return true;
                }

                if (p.Category.ParentCategory != null)
                {
                    var parentName = (p.Category.ParentCategory.Name ?? "").ToLower();
                    if (parentName.Contains("phụ kiện") || parentName.Contains("tai nghe") || 
                        parentName.Contains("cáp") || parentName.Contains("sạc") || 
                        parentName.Contains("ốp") || parentName.Contains("kính"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
