using ECommerce.Models;
using ECommerce1.DTOs.ProductVariant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductVariantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= READ: Lấy danh sách =================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? productId)
        {
            var query = _context.ProductVariants.AsQueryable();

            // Cho phép lọc Variant theo ID Sản phẩm gốc (Ví dụ: Lấy tất cả màu của iPhone 15)
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
            // Kiểm tra Product gốc có tồn tại không
            if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
                return BadRequest("Sản phẩm gốc (ProductId) không tồn tại.");

            var newVariant = new ProductVariant
            {
                Name = request.Name,
                Sku = !string.IsNullOrEmpty(request.Sku) ? request.Sku : GenerateSku(request.Name),
                Price = request.Price,
                TotalStock = request.TotalStock,
                ReservedStock = 0, // Mới tạo thì chưa có ai mua
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
            if (!await _context.Products.AnyAsync(p => p.Id == productId))
                return BadRequest("Sản phẩm gốc không tồn tại.");

            var newVariants = requests.Select(request => new ProductVariant
            {
                Name = request.Name,
                Sku = !string.IsNullOrEmpty(request.Sku) ? request.Sku : GenerateSku(request.Name),
                Price = request.Price,
                TotalStock = request.TotalStock,
                ReservedStock = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ProductId = request.ProductId,
                ImageId = request.ImageId ?? "",
                Attributes = request.Attributes ?? "{}",
                IsActive = request.IsActive
            }).ToList();

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

            variant.Name = request.Name;
            variant.Sku = !string.IsNullOrEmpty(request.Sku) ? request.Sku : GenerateSku(request.Name);
            variant.Price = request.Price;
            
            // Tương tự, nếu quản lý kho phức tạp thì Update Stock nên dùng API riêng
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
            if (!await _context.Products.AnyAsync(p => p.Id == productId))
                return BadRequest("Sản phẩm gốc không tồn tại.");

            // 1. Lấy tất cả biến thể cũ từ DB
            var existingVariants = await _context.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .Include(pv => pv.OrderItems)
                .ToListAsync();

            // Danh sách ID từ Frontend gửi lên
            var incomingIds = requests.Select(r => r.Id).Where(id => id > 0).ToList();

            // 2. Xóa (DELETE) những biến thể không còn trong danh sách gửi lên
            var toDelete = existingVariants.Where(ev => !incomingIds.Contains(ev.Id)).ToList();
            foreach (var variant in toDelete)
            {
                if (variant.OrderItems != null && variant.OrderItems.Any())
                {
                    return BadRequest($"Không thể xóa biến thể '{variant.Name}' vì đã nằm trong lịch sử đơn hàng của khách.");
                }
                _context.ProductVariants.Remove(variant);
            }

            // 3. Upsert (INSERT & UPDATE)
            foreach (var req in requests)
            {
                if (req.Id == 0)
                {
                    // Thêm mới
                    var newVariant = new ProductVariant
                    {
                        Name = req.Name,
                        Sku = !string.IsNullOrEmpty(req.Sku) ? req.Sku : GenerateSku(req.Name),
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
                    // Cập nhật
                    var existing = existingVariants.FirstOrDefault(ev => ev.Id == req.Id);
                    if (existing != null)
                    {
                        existing.Name = req.Name;
                        existing.Sku = !string.IsNullOrEmpty(req.Sku) ? req.Sku : GenerateSku(req.Name);
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
                .Include(pv => pv.OrderItems) // Kiểm tra xem Variant này đã có người mua chưa
                .FirstOrDefaultAsync(pv => pv.Id == id);

            if (variant == null)
                return NotFound("Không tìm thấy biến thể sản phẩm.");

            if (variant.OrderItems != null && variant.OrderItems.Any())
                return BadRequest("Không thể xóa biến thể này vì đã nằm trong lịch sử đơn hàng của khách.");

            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();

            return Ok("Xóa biến thể sản phẩm thành công.");
        }

        private string GenerateSku(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            
            // Remove diacritics
            string temp = name;
            string[] VietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
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
            
            // Replace non-alphanumeric (except hyphens and spaces)
            var chars = temp.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '-' && chars[i] != ' ')
                {
                    chars[i] = '-';
                }
            }
            string result = new string(chars);
            
            // Replace multiple spaces/hyphens with a single hyphen
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[\s\-]+", "-");
            
            return result.ToUpper().Trim('-');
        }
    }
}
