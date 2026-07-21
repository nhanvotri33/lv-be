using ECommerce.Models;
using ECommerce1.DTOs.ProductVariant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ProductVariantService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IEnumerable<ProductVariantResponse>> GetAllAsync(int? productId)
        {
            var query = _context.ProductVariants.AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(pv => pv.ProductId == productId.Value);
            }

            var variants = await query
                .Select(pv => new ProductVariantResponse
                {
                    Id = pv.Id,
                    Name = pv.Name,
                    Sku = pv.Sku,
                    Price = pv.Price,
                    TotalStock = pv.TotalStock,
                    ReservedStock = pv.ReservedStock,
                    AvailableStock = pv.AvailableStock,
                    CreatedAt = pv.CreatedAt,
                    UpdatedAt = pv.UpdatedAt,
                    ProductId = pv.ProductId,
                    ImageId = pv.ImageId,
                    Attributes = pv.Attributes,
                    SpecsOverride = pv.SpecsOverride,
                    IsActive = pv.IsActive
                })
                .ToListAsync();

            return variants;
        }

        public async Task<ProductVariantResponse> GetByIdAsync(int id)
        {
            var pv = await _context.ProductVariants.FindAsync(id);
            if (pv == null)
                throw new KeyNotFoundException("Không tìm thấy biến thể (Variant) này.");

            return new ProductVariantResponse
            {
                Id = pv.Id,
                Name = pv.Name,
                Sku = pv.Sku,
                Price = pv.Price,
                TotalStock = pv.TotalStock,
                ReservedStock = pv.ReservedStock,
                AvailableStock = pv.AvailableStock,
                CreatedAt = pv.CreatedAt,
                UpdatedAt = pv.UpdatedAt,
                ProductId = pv.ProductId,
                ImageId = pv.ImageId,
                Attributes = pv.Attributes,
                SpecsOverride = pv.SpecsOverride,
                IsActive = pv.IsActive
            };
        }

        public async Task CreateAsync(ProductVariantRequest request)
        {
            if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
                throw new KeyNotFoundException("Sản phẩm gốc (ProductId) không tồn tại.");

            ValidateAttributes(request.Attributes);

            string finalSku = !string.IsNullOrEmpty(request.Sku) 
                ? request.Sku.Trim().ToUpper() 
                : await GenerateSkuAsync(request.ProductId, request.Attributes);

            if (!string.IsNullOrEmpty(finalSku) && await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku))
                throw new ArgumentException($"Mã SKU '{finalSku}' đã tồn tại ở một biến thể khác.");

            var newVariant = new ProductVariant
            {
                Name = request.Name,
                Sku = finalSku,
                Price = request.Price,
                TotalStock = request.TotalStock,
                ReservedStock = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ProductId = request.ProductId,
                ImageId = request.ImageId ?? "",
                Attributes = request.Attributes ?? "{}",
                SpecsOverride = request.SpecsOverride,
                IsActive = request.IsActive
            };

            _context.ProductVariants.Add(newVariant);
            await _context.SaveChangesAsync();

            // Tự động sinh Phiếu nhập kho ban đầu (Xử lý ngầm) nếu tồn kho biến thể > 0
            if (newVariant.TotalStock > 0)
            {
                var initTx = new InventoryTransaction
                {
                    VariantId = newVariant.Id,
                    QuantityChanged = newVariant.TotalStock,
                    TransactionType = "IMPORT",
                    Price = newVariant.Price,
                    Note = "Khởi tạo tồn kho ban đầu",
                    IsReverted = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.InventoryTransactions.Add(initTx);
                await _context.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Action = "IMPORT",
                    TargetTable = "InventoryTransactions",
                    TargetId = initTx.Id.ToString(),
                    NewValues = $"Khởi tạo tồn kho ban đầu: +{newVariant.TotalStock} cho biến thể '{newVariant.Name}' (SKU: {newVariant.Sku})",
                    Timestamp = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }

            await SyncParentProductStockAsync(request.ProductId);
        }

        public async Task CreateBatchAsync(List<ProductVariantRequest> requests)
        {
            if (requests == null || !requests.Any())
                throw new ArgumentException("Danh sách biến thể trống.");

            var productId = requests.First().ProductId;
            var product = await _context.Products.Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                throw new KeyNotFoundException("Sản phẩm gốc không tồn tại.");

            string brandCode = product.Brand != null && !string.IsNullOrEmpty(product.Brand.BrandCode) ? product.Brand.BrandCode : "GEN";
            string productCode = !string.IsNullOrEmpty(product.ProductCode) ? product.ProductCode : "PROD";

            var newVariants = new List<ProductVariant>();

            foreach (var request in requests)
            {
                ValidateAttributes(request.Attributes);

                string finalSku = request.Sku;
                if (string.IsNullOrEmpty(finalSku))
                {
                    var attrParts = new List<string>();
                    if (!string.IsNullOrEmpty(request.Attributes))
                    {
                        try
                        {
                            var attrs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(request.Attributes);
                            if (attrs != null)
                            {
                                var sortedKeys = attrs.Keys
                                    .Where(k => k != "costPrice" && k != "chargeTax")
                                    .OrderBy(k => k)
                                    .ToList();

                                foreach (var key in sortedKeys)
                                {
                                    string value = attrs[key];
                                    string processedVal = ProcessAttributeValue(key, value);
                                    if (!string.IsNullOrEmpty(processedVal))
                                    {
                                        attrParts.Add(processedVal);
                                    }
                                }
                            }
                        }
                        catch {}
                    }
                    string suffix = attrParts.Count > 0 ? string.Join("-", attrParts) : string.Empty;
                    finalSku = !string.IsNullOrEmpty(suffix) ? $"{brandCode}-{productCode}-{suffix}" : $"{brandCode}-{productCode}";
                }
                finalSku = finalSku.Trim().ToUpper();

                if (!string.IsNullOrEmpty(finalSku) && (newVariants.Any(nv => nv.Sku == finalSku) || await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku)))
                    throw new ArgumentException($"Mã SKU '{finalSku}' bị trùng lặp.");

                newVariants.Add(new ProductVariant
                {
                    Name = request.Name,
                    Sku = finalSku,
                    Price = request.Price,
                    TotalStock = request.TotalStock,
                    ReservedStock = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProductId = request.ProductId,
                    ImageId = request.ImageId ?? "",
                    Attributes = request.Attributes ?? "{}",
                    SpecsOverride = request.SpecsOverride,
                    IsActive = request.IsActive
                });
            }

            _context.ProductVariants.AddRange(newVariants);
            await _context.SaveChangesAsync();
            await SyncParentProductStockAsync(productId);
        }

        public async Task UpdateAsync(int id, ProductVariantRequest request)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null)
                throw new KeyNotFoundException("Không tìm thấy biến thể sản phẩm.");

            if (variant.ProductId != request.ProductId)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
                    throw new KeyNotFoundException("Sản phẩm gốc (ProductId) không tồn tại.");
            }

            ValidateAttributes(request.Attributes);

            string finalSku = !string.IsNullOrEmpty(request.Sku) 
                ? request.Sku.Trim().ToUpper() 
                : await GenerateSkuAsync(request.ProductId, request.Attributes);

            if (!string.IsNullOrEmpty(finalSku) && await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku && pv.Id != id))
                throw new ArgumentException($"Mã SKU '{finalSku}' đã tồn tại ở một biến thể khác.");

            if (variant.ImageId != request.ImageId)
            {
                _fileService.DeleteImage(variant.ImageId);
            }

            int oldProductId = variant.ProductId;
            int newProductId = request.ProductId;

            variant.Name = request.Name;
            variant.Sku = finalSku;
            variant.Price = request.Price;
            variant.TotalStock = request.TotalStock;
            variant.ProductId = request.ProductId;
            variant.ImageId = request.ImageId ?? "";
            variant.Attributes = request.Attributes ?? "{}";
            variant.SpecsOverride = request.SpecsOverride;
            variant.IsActive = request.IsActive;
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await SyncParentProductStockAsync(oldProductId);
            if (newProductId != oldProductId)
            {
                await SyncParentProductStockAsync(newProductId);
            }
        }

        public async Task SyncAsync(int productId, List<ProductVariantRequest> requests)
        {
            var product = await _context.Products.Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                throw new KeyNotFoundException("Sản phẩm gốc không tồn tại.");

            string brandCode = product.Brand != null && !string.IsNullOrEmpty(product.Brand.BrandCode) ? product.Brand.BrandCode : "GEN";
            string productCode = !string.IsNullOrEmpty(product.ProductCode) ? product.ProductCode : "PROD";

            var existingVariants = await _context.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .Include(pv => pv.OrderItems)
                .ToListAsync();

            var incomingIds = requests.Select(r => r.Id).Where(id => id > 0).ToList();

            // 1. Delete old variants not in the request list
            var toDelete = existingVariants.Where(ev => !incomingIds.Contains(ev.Id)).ToList();
            foreach (var variant in toDelete)
            {
                if (variant.OrderItems != null && variant.OrderItems.Any())
                    throw new ArgumentException($"Không thể xóa biến thể '{variant.Name}' vì đã nằm trong lịch sử đơn hàng của khách.");

                if (!string.IsNullOrEmpty(variant.ImageId))
                {
                    _fileService.DeleteImage(variant.ImageId);
                }
                _context.ProductVariants.Remove(variant);
            }

            // 2. Add or Update
            foreach (var request in requests)
            {
                ValidateAttributes(request.Attributes);

                string finalSku = request.Sku;
                if (string.IsNullOrEmpty(finalSku))
                {
                    var attrParts = new List<string>();
                    if (!string.IsNullOrEmpty(request.Attributes))
                    {
                        try
                        {
                            var attrs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(request.Attributes);
                            if (attrs != null)
                            {
                                var sortedKeys = attrs.Keys
                                    .Where(k => k != "costPrice" && k != "chargeTax")
                                    .OrderBy(k => k)
                                    .ToList();

                                foreach (var key in sortedKeys)
                                {
                                    string value = attrs[key];
                                    string processedVal = ProcessAttributeValue(key, value);
                                    if (!string.IsNullOrEmpty(processedVal))
                                    {
                                        attrParts.Add(processedVal);
                                    }
                                }
                            }
                        }
                        catch {}
                    }
                    string suffix = attrParts.Count > 0 ? string.Join("-", attrParts) : string.Empty;
                    finalSku = !string.IsNullOrEmpty(suffix) ? $"{brandCode}-{productCode}-{suffix}" : $"{brandCode}-{productCode}";
                }
                finalSku = finalSku.Trim().ToUpper();

                if (request.Id > 0)
                {
                    // Update
                    var existing = existingVariants.FirstOrDefault(ev => ev.Id == request.Id);
                    if (existing != null)
                    {
                        if (existing.Sku.ToUpper() != finalSku && await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku && pv.Id != existing.Id))
                            throw new ArgumentException($"Mã SKU '{finalSku}' đã tồn tại ở một biến thể khác.");

                        if (existing.ImageId != request.ImageId)
                        {
                            _fileService.DeleteImage(existing.ImageId);
                        }

                        existing.Name = request.Name;
                        existing.Sku = finalSku;
                        existing.Price = request.Price;
                        existing.TotalStock = request.TotalStock;
                        existing.ImageId = request.ImageId ?? "";
                        existing.Attributes = request.Attributes ?? "{}";
                        existing.SpecsOverride = request.SpecsOverride;
                        existing.IsActive = request.IsActive;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // Add new
                    if (await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku))
                        throw new ArgumentException($"Mã SKU '{finalSku}' đã tồn tại ở một biến thể khác.");

                    var newV = new ProductVariant
                    {
                        Name = request.Name,
                        Sku = finalSku,
                        Price = request.Price,
                        TotalStock = request.TotalStock,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ProductId = productId,
                        ImageId = request.ImageId ?? "",
                        Attributes = request.Attributes ?? "{}",
                        SpecsOverride = request.SpecsOverride,
                        IsActive = request.IsActive
                    };
                    _context.ProductVariants.Add(newV);
                    await _context.SaveChangesAsync();

                    if (newV.TotalStock > 0)
                    {
                        var initTx = new InventoryTransaction
                        {
                            VariantId = newV.Id,
                            QuantityChanged = newV.TotalStock,
                            TransactionType = "IMPORT",
                            Price = newV.Price,
                            Note = "Khởi tạo tồn kho ban đầu",
                            IsReverted = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryTransactions.Add(initTx);

                        var auditLog = new AuditLog
                        {
                            Action = "IMPORT",
                            TargetTable = "InventoryTransactions",
                            TargetId = initTx.Id.ToString(),
                            NewValues = $"Khởi tạo tồn kho ban đầu: +{newV.TotalStock} cho biến thể '{newV.Name}' (SKU: {newV.Sku})",
                            Timestamp = DateTime.UtcNow
                        };
                        _context.AuditLogs.Add(auditLog);
                    }
                }
            }

            await _context.SaveChangesAsync();
            await SyncParentProductStockAsync(productId);
        }

        public async Task DeleteAsync(int id)
        {
            var variant = await _context.ProductVariants
                .Include(pv => pv.OrderItems)
                .FirstOrDefaultAsync(pv => pv.Id == id);

            if (variant == null)
                throw new KeyNotFoundException("Không tìm thấy biến thể sản phẩm.");

            if (variant.OrderItems != null && variant.OrderItems.Any())
                throw new ArgumentException("Không thể xóa biến thể này vì đã nằm trong lịch sử đơn hàng của khách.");

            if (!string.IsNullOrEmpty(variant.ImageId))
            {
                _fileService.DeleteImage(variant.ImageId);
            }

            int productId = variant.ProductId;
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
            await SyncParentProductStockAsync(productId);
        }

        private async Task<string> GenerateSkuAsync(int productId, string attributesJson)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return string.Empty;

            string brandCode = product.Brand != null && !string.IsNullOrEmpty(product.Brand.BrandCode) 
                ? product.Brand.BrandCode 
                : "GEN";

            string productCode = !string.IsNullOrEmpty(product.ProductCode) 
                ? product.ProductCode 
                : "PROD";

            var attrParts = new List<string>();

            if (!string.IsNullOrEmpty(attributesJson))
            {
                try
                {
                    var attrs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(attributesJson);
                    if (attrs != null)
                    {
                        var sortedKeys = attrs.Keys
                            .Where(k => k != "costPrice" && k != "chargeTax")
                            .OrderBy(k => k)
                            .ToList();

                        foreach (var key in sortedKeys)
                        {
                            string value = attrs[key];
                            string processedVal = ProcessAttributeValue(key, value);
                            if (!string.IsNullOrEmpty(processedVal))
                            {
                                attrParts.Add(processedVal);
                            }
                        }
                    }
                }
                catch {}
            }

            string suffix = attrParts.Count > 0 ? string.Join("-", attrParts) : string.Empty;
            string finalSku = !string.IsNullOrEmpty(suffix) 
                ? $"{brandCode}-{productCode}-{suffix}" 
                : $"{brandCode}-{productCode}";

            return finalSku.ToUpper();
        }

        private void ValidateAttributes(string attributesJson)
        {
            if (string.IsNullOrEmpty(attributesJson)) return;

            try
            {
                var attrs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(attributesJson);
                if (attrs != null)
                {
                    foreach (var kvp in attrs)
                    {
                        string name = kvp.Key;
                        string value = kvp.Value;

                        if (name == "Màu sắc" || name == "Kích thước")
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), @"^\d+$"))
                            {
                                throw new ArgumentException($"Thuộc tính '{name}' không được phép chỉ chứa toàn các con số.");
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                throw new ArgumentException("Chuỗi thuộc tính JSON không hợp lệ.");
            }
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string temp = text;
            string[] VietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôồốộổỗơờớợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưừứựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                {
                    temp = temp.Replace(VietnameseSigns[i][j].ToString(), VietnameseSigns[0][i - 1].ToString());
                }
            }
            return temp;
        }

        private string ProcessAttributeValue(string attrName, string attrValue)
        {
            if (string.IsNullOrEmpty(attrValue)) return string.Empty;

            string cleanVal = System.Text.RegularExpressions.Regex.Replace(attrValue.Trim(), @"\s+", " ");

            if (attrName.Contains("Dung lượng") || attrName.Contains("RAM") || attrName.Contains("ROM"))
            {
                var digits = cleanVal.Where(char.IsDigit).ToArray();
                return new string(digits);
            }

            var words = cleanVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
            {
                string unsigned = RemoveDiacritics(words[0]);
                var lettersAndDigits = unsigned.Where(char.IsLetterOrDigit).ToArray();
                string result = new string(lettersAndDigits).ToUpper();
                return result.Length > 5 ? result.Substring(0, 5) : result;
            }
            else if (words.Length > 1)
            {
                var firstLetters = words.Select(w => {
                    string unsigned = RemoveDiacritics(w);
                    var validChars = unsigned.Where(char.IsLetterOrDigit).ToArray();
                    return validChars.Length > 0 ? validChars[0] : '\0';
                }).Where(c => c != '\0').ToArray();

                string result = new string(firstLetters).ToUpper();
                return result.Length > 10 ? result.Substring(0, 10) : result;
            }

            return string.Empty;
        }
        private async Task SyncParentProductStockAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var variants = await _context.ProductVariants
                    .Where(pv => pv.ProductId == productId)
                    .ToListAsync();
                
                product.TotalStock = variants.Sum(pv => pv.TotalStock);
                product.ReservedStock = variants.Sum(pv => pv.ReservedStock);
                await _context.SaveChangesAsync();
            }
        }
    }
}
