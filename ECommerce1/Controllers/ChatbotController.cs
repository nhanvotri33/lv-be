using ECommerce.Models;
using ECommerce1.Services.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        // Danh sách các từ khóa ngoài lề cần chặn ngay lập tức (không gọi API OpenAI để tiết kiệm token)
        private static readonly string[] OffTopicKeywords = new[]
        {
            "cấu trúc dữ liệu", "thuật giải", "thuật toán", "lập trình", "viết code", "viết phần mềm",
            "bài tập", "giải toán", "nấu ăn", "công thức", "thời tiết", "chính trị", "lịch sử thế giới",
            "làm thơ", "viết văn", "đóng vai", "bài văn", "python", "java", "c#", "cpp", "javascript",
            "gpt", "openai", "dịch tiếng", "dịch thuật"
        };

        public ChatbotController(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Vui lòng nhập nội dung cần tư vấn.");

            // 1. CHẶN NGAY CÂU HỎI NGOÀI LỀ Ở BACKEND (Không gọi API OpenAI -> Tiết kiệm 100% chi phí Token)
            if (IsOffTopicQuery(request.Message))
            {
                return Ok(new ChatbotResponse
                {
                    Reply = "Dạ xin lỗi bạn, mình là Trợ lý AI của PhoneShop nên chỉ hỗ trợ tư vấn các thông tin về điện thoại, phụ kiện, khuyến mãi và chính sách mua hàng của cửa hàng thôi ạ! Bạn cần mình hỗ trợ thông tin sản phẩm hay chương trình giảm giá nào không ạ?"
                });
            }

            if (!_aiService.IsConfigured)
                return StatusCode(503, "Chatbot chưa được cấu hình API key.");

            // 2. Nạp ngữ cảnh sản phẩm, phụ kiện & khuyến mãi thực tế từ DB
            var productContext = await BuildProductContextAsync(request.Message, cancellationToken);
            var reply = await _aiService.ChatAsync(request.Message, request.History, productContext, cancellationToken);

            return Ok(new ChatbotResponse { Reply = reply });
        }

        private static bool IsOffTopicQuery(string message)
        {
            var lower = message.ToLowerInvariant();
            
            // Nếu chứa bất kỳ từ khóa ngoài lề nào
            foreach (var kw in OffTopicKeywords)
            {
                if (lower.Contains(kw))
                {
                    // Nếu câu hỏi không chứa các từ khóa liên quan đến cửa hàng (điện thoại, phụ kiện, mua, giá, bảo hành)
                    if (!lower.Contains("điện thoại") && !lower.Contains("phụ kiện") && !lower.Contains("iphone") && 
                        !lower.Contains("samsung") && !lower.Contains("ốp") && !lower.Contains("sạc") && 
                        !lower.Contains("bảo hành") && !lower.Contains("giảm giá") && !lower.Contains("khuyến mãi"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<string> BuildProductContextAsync(string message, CancellationToken cancellationToken)
        {
            var keywords = message
                .ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', ';', ':', '-', '_', '/', '\\', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length >= 2)
                .Distinct()
                .Take(8)
                .ToList();

            // Lấy danh sách sản phẩm (Bao gồm cả Điện thoại và Phụ kiện)
            var products = await _context.Products
                .Include(product => product.Brand)
                .Include(product => product.Category)
                .Where(product => product.IsActive)
                .OrderByDescending(product => product.IsFeatured)
                .ThenByDescending(product => product.CreatedAt)
                .Take(100)
                .Select(product => new
                {
                    product.Id,
                    product.Name,
                    product.BasePrice,
                    product.TotalStock,
                    product.ReservedStock,
                    BrandName = product.Brand != null ? product.Brand.Name : "",
                    CategoryName = product.Category != null ? product.Category.Name : ""
                })
                .ToListAsync(cancellationToken);

            var rankedProducts = products
                .Select(product => new
                {
                    Product = product,
                    Score = keywords.Count(keyword =>
                        product.Name.ToLowerInvariant().Contains(keyword) ||
                        product.BrandName.ToLowerInvariant().Contains(keyword) ||
                        product.CategoryName.ToLowerInvariant().Contains(keyword))
                })
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Product.BasePrice)
                .Take(15)
                .Select(item =>
                {
                    var product = item.Product;
                    var stock = product.TotalStock - product.ReservedStock;
                    return $"- #{product.Id} [{product.CategoryName}] {product.Name} | Hãng: {product.BrandName} | Giá từ: {product.BasePrice.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))}đ | Tồn: {(stock > 0 ? "còn hàng" : "hết hàng")}";
                });

            // Lấy danh sách Chương trình Khuyến mãi đang Active từ DB
            var activePromotions = await _context.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.UtcNow)
                .Take(5)
                .Select(p => $"- Mã [{p.Code}]: Loại giảm ({p.DiscountType}), Giá trị giảm ({p.DiscountValue})")
                .ToListAsync(cancellationToken);

            var contextText = "--- NGỮ CẢNH SẢN PHẨM & PHỤ KIỆN TÌM THẤY ---\n" + string.Join("\n", rankedProducts);

            if (activePromotions.Any())
            {
                contextText += "\n\n--- MÃ GIẢM GIÁ & KHUYẾN MÃI ĐANG ÁP DỤNG ---\n" + string.Join("\n", activePromotions);
            }

            return contextText;
        }
    }
}
