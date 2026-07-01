using ECommerce.Models;
using ECommerce1.DTOs.ProductVariant;
using ECommerce1.Services;
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
    public class ProductVariantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ProductVariantController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // ================= READ: Lấy danh sách =================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? productId)
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
                    IsActive = pv.IsActive
                })
                .ToListAsync();

            return Ok(variants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pv = await _context.ProductVariants.FindAsync(id);

            if (pv == null)
                return NotFound("Không tìm thấy biến thể (Variant) này.");

            return Ok(new ProductVariantResponse
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
                IsActive = pv.IsActive
            });
        }

        // ================= CREATE: Cần đăng nhập =================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductVariantRequest request)
        {
            if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
                return BadRequest("Sản phẩm gốc (ProductId) không tồn tại.");

            if (!ValidateAttributes(request.Attributes, out string valError))
            {
                return BadRequest(valError);
            }

            string finalSku = !string.IsNullOrEmpty(request.Sku) 
                ? request.Sku.Trim().ToUpper() 
                : await GenerateSkuAsync(request.ProductId, request.Attributes);

            if (!string.IsNullOrEmpty(finalSku) && await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku))
            {
                return BadRequest($"Mã SKU '{finalSku}' đã tồn tại ở một biến thể khác.");
            }

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
                IsActive = request.IsActive
            };

            _context.ProductVariants.Add(newVariant);
            await _context.SaveChangesAsync();

            return Ok("Tạo biến thể sản phẩm thành công.");
        }

        // ================= CREATE BATCH: Tạo nhiều biến thể cùng lúc =================
        [HttpPost("batch")]
        [Authorize]
        public async Task<IActionResult> CreateBatch([FromBody] List<ProductVariantRequest> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest("Danh sách biến thể trống.");

            var productId = requests.First().ProductId;
            var product = await _context.Products.Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return BadRequest("Sản phẩm gốc không tồn tại.");

            string brandCode = product.Brand != null && !string.IsNullOrEmpty(product.Brand.BrandCode) ? product.Brand.BrandCode : "GEN";
            string productCode = !string.IsNullOrEmpty(product.ProductCode) ? product.ProductCode : "PROD";

            var newVariants = new List<ProductVariant>();

            foreach (var request in requests)
            {
                if (!ValidateAttributes(request.Attributes, out string valError))
                {
                    return BadRequest(valError);
                }

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
                {
                    return BadRequest($"Mã SKU '{finalSku}' bị trùng lặp.");
                }

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
                    IsActive = request.IsActive
                });
            }

            _context.ProductVariants.AddRange(newVariants);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Tạo thành công {newVariants.Count} biến thể." });
        }

        // ================= UPDATE: Cần đăng nhập =================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductVariantRequest request)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null)
                return NotFound("Không tìm thấy biến thể sản phẩm.");

            if (variant.ProductId != request.ProductId)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
                    return BadRequest("Sản phẩm gốc (ProductId) không tồn tại.");
            }

            if (!ValidateAttributes(request.Attributes, out string valError))
            {
                return BadRequest(valError);
            }

            string finalSku = !string.IsNullOrEmpty(request.Sku) 
                ? request.Sku.Trim().ToUpper() 
                : await GenerateSkuAsync(request.ProductId, request.Attributes);

            if (!string.IsNullOrEmpty(finalSku) && await _context.ProductVariants.AnyAsync(pv => pv.Sku.ToUpper() == finalSku && pv.Id != id))
            {
                return BadRequest($"Mã SKU '{finalSku}' đã tồn tại ở một biến thể khác.");
            }

            if (variant.ImageId != request.ImageId)
            {
                _fileService.DeleteImage(variant.ImageId);
            }

            variant.Name = request.Name;
            variant.Sku = finalSku;
            variant.Price = request.Price;
            variant.TotalStock = request.TotalStock;
            variant.ProductId = request.ProductId;
            variant.ImageId = request.ImageId ?? "";
            variant.Attributes = request.Attributes ?? "{}";
            variant.IsActive = request.IsActive;
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật biến thể sản phẩm thành công.");
        }

        // ================= SYNC: Đồng bộ Upsert & Delete =================
        [HttpPut("sync/{productId}")]
        [Authorize]
        public async Task<IActionResult> Sync(int productId, [FromBody] List<ProductVariantRequest> requests)
        {
            var product = await _context.Products.Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return BadRequest("Sản phẩm gốc không tồn tại.");

            string brandCode = product.Brand != null && !string.IsNullOrEmpty(product.Brand.BrandCode) ? product.Brand.BrandCode : "GEN";
            string productCode = !string.IsNullOrEmpty(product.ProductCode) ? product.ProductCode : "PROD";

            var existingVariants = await _context.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .Include(pv => pv.OrderItems)
                .ToListAsync();

            var incomingIds = requests.Select(r => r.Id).Where(id => id > 0).ToList();

            var toDelete = existingVariants.Where(ev => !incomingIds.Contains(ev.Id)).ToList();
            foreach (var variant in toDelete)
            {
                if (variant.OrderItems != null && variant.OrderItems.Any())
                {
                    return BadRequest($"Không thể xóa biến thể '{variant.Name}' vì đã nằm trong lịch sử đơn hàng của khách.");
                }
                if (!string.IsNullOrEmpty(variant.ImageId))
                {
                    _fileService.DeleteImage(variant.ImageId);
                }
                _context.ProductVariants.Remove(variant);
            }

            var skuMap = new Dictionary<string, string>();

            foreach (var req in requests)
            {
                if (!ValidateAttributes(req.Attributes, out string valError))
                {
                    return BadRequest(valError);
                }

                string finalSku = req.Sku;
                if (string.IsNullOrEmpty(finalSku))
                {
                    var attrParts = new List<string>();
                    if (!string.IsNullOrEmpty(req.Attributes))
                    {
                        try
                        {
                            var attrs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(req.Attributes);
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

                if (skuMap.ContainsKey(finalSku))
                {
                    return BadRequest($"Mã SKU '{finalSku}' bị trùng lặp trong danh sách đồng bộ.");
                }
                skuMap.Add(finalSku, req.Name);

                bool existsInDb = await _context.ProductVariants
                    .AnyAsync(pv => pv.Sku.ToUpper() == finalSku && pv.ProductId != productId && (req.Id == 0 || pv.Id != req.Id));
                if (existsInDb)
                {
                    return BadRequest($"Mã SKU '{finalSku}' đã được sử dụng bởi sản phẩm khác.");
                }

                req.Sku = finalSku;
            }

            foreach (var req in requests)
            {
                if (req.Id == 0)
                {
                    var newVariant = new ProductVariant
                    {
                        Name = req.Name,
                        Sku = req.Sku,
                        Price = req.Price,
                        TotalStock = req.TotalStock,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ProductId = productId,
                        ImageId = req.ImageId ?? "",
                        Attributes = req.Attributes ?? "{}",
                        IsActive = req.IsActive
                    };
                    _context.ProductVariants.Add(newVariant);
                }
                else
                {
                    var existing = existingVariants.FirstOrDefault(ev => ev.Id == req.Id);
                    if (existing != null)
                    {
                        if (existing.ImageId != req.ImageId)
                        {
                            _fileService.DeleteImage(existing.ImageId);
                        }
                        existing.Name = req.Name;
                        existing.Sku = req.Sku;
                        existing.Price = req.Price;
                        existing.TotalStock = req.TotalStock;
                        existing.ImageId = req.ImageId ?? "";
                        existing.Attributes = req.Attributes ?? "{}";
                        existing.IsActive = req.IsActive;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đồng bộ biến thể thành công." });
        }

        // ================= DELETE: Cần đăng nhập =================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var variant = await _context.ProductVariants
                .Include(pv => pv.OrderItems)
                .FirstOrDefaultAsync(pv => pv.Id == id);

            if (variant == null)
                return NotFound("Không tìm thấy biến thể sản phẩm.");

            if (variant.OrderItems != null && variant.OrderItems.Any())
                return BadRequest("Không thể xóa biến thể này vì đã nằm trong lịch sử đơn hàng của khách.");

            if (!string.IsNullOrEmpty(variant.ImageId))
            {
                _fileService.DeleteImage(variant.ImageId);
            }

            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();

            return Ok("Xóa biến thể sản phẩm thành công.");
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
                catch
                {
                    // Ignore
                }
            }

            string suffix = attrParts.Count > 0 ? string.Join("-", attrParts) : string.Empty;
            string finalSku = !string.IsNullOrEmpty(suffix) 
                ? $"{brandCode}-{productCode}-{suffix}" 
                : $"{brandCode}-{productCode}";

            return finalSku.ToUpper();
        }

        private bool ValidateAttributes(string attributesJson, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(attributesJson)) return true;

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
                                errorMessage = $"Thuộc tính '{name}' không được phép chỉ chứa toàn các con số.";
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return true;
        }
    }
}
