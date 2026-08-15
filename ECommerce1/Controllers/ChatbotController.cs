// ==========================================================================
// MODULE: ChatbotController.cs
// MỤC ĐÍCH: API Controller kết nối Chatbot AI trợ lý tư vấn sản phẩm (OpenAI GPT).
// ==========================================================================
using ECommerce.Models;
using ECommerce1.Services.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
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

        //tránh làm tốn token 
        // Câu chào kết thuần túy. Chỉ khớp TUYỆT ĐỐI cả câu (sau khi chuẩn hóa), không dùng Contains,
        // vì "cảm ơn bạn, cho mình hỏi iPhone 15 còn không" là câu hỏi thật chứ không phải lời chào kết.
        private static readonly HashSet<string> FarewellMessages = new(StringComparer.OrdinalIgnoreCase)
        {
            "end", "bye", "byebye", "bye bye", "goodbye", "tạm biệt", "tam biet",
            "cảm ơn", "cam on", "cám ơn", "cảm ơn bạn", "cám ơn bạn", "cảm ơn nhé", "cảm ơn nha",
            "thanks", "thank you", "tks", "thank kiu", "ok cảm ơn", "oke cảm ơn"
        };

        private static bool IsFarewell(string message)
        {
            var normalized = message.Trim().ToLowerInvariant().Trim('.', '!', '?', ',', ' ');
            return FarewellMessages.Contains(normalized);
        }

        public ChatbotController(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        // [API Endpoint POST [Route: `chat`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPost("chat")]
        // [Hàm thực thi nghiệp vụ]: `Chat` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Chat([FromBody] ChatbotRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Vui lòng nhập nội dung cần tư vấn.");

            // 1. ĐÓNG HỘI THOẠI: khách chỉ chào kết ("cảm ơn", "bye", "end") thì chào lại luôn,
            // không gọi API OpenAI và không hỏi thêm nước đôi.
            if (IsFarewell(request.Message))
            {
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new ChatbotResponse
                {
                    Reply = "Dạ cảm ơn bạn đã ghé PhoneShop! Chúc bạn một ngày tốt lành, khi nào cần tư vấn điện thoại hay phụ kiện thì cứ nhắn mình nhé! 😊"
                });
            }

            // 2. CHẶN NGAY CÂU HỎI NGOÀI LỀ Ở BACKEND (Không gọi API OpenAI -> Tiết kiệm 100% chi phí Token)
            if (IsOffTopicQuery(request.Message))
            {
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new ChatbotResponse
                {
                    Reply = "Dạ xin lỗi bạn, mình là Trợ lý AI của PhoneShop nên chỉ hỗ trợ tư vấn các thông tin về điện thoại, phụ kiện, khuyến mãi và chính sách mua hàng của cửa hàng thôi ạ! Bạn cần mình hỗ trợ thông tin sản phẩm hay chương trình giảm giá nào không ạ?"
                });
            }

            // 3. Nạp ngữ cảnh sản phẩm, phụ kiện & khuyến mãi thực tế từ DB
            var productContext = await BuildProductContextAsync(request.Message, request.History, cancellationToken);

            if (!_aiService.IsConfigured)
            {
                // FALLBACK THÔNG MINH KHI CHƯA NẠP KEY AI:
                // Tìm kiếm trực tiếp sản phẩm trong CSDL và trả về thông tin giá cả, tồn kho cho người dùng
                var localReply = BuildLocalDatabaseReply(request.Message, productContext);
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new ChatbotResponse { Reply = localReply });
            }

            var reply = await _aiService.ChatAsync(request.Message, request.History, productContext, cancellationToken);
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
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

        /// <summary>
        /// Attributes của biến thể là chuỗi JSON dạng {"Màu sắc":"Black Titanium","Dung lượng":"256GB"}.
        /// Chuyển thành dạng đọc được để nhét vào ngữ cảnh cho AI, ví dụ: "Màu sắc: Black Titanium, Dung lượng: 256GB".
        /// </summary>
        private static string DescribeAttributes(string? attributesJson)
        {
            if (string.IsNullOrWhiteSpace(attributesJson))
                return string.Empty;

            try
            {
                using var document = JsonDocument.Parse(attributesJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return string.Empty;

                var parts = document.RootElement
                    .EnumerateObject()
                    .Where(property => property.Value.ValueKind == JsonValueKind.String)
                    .Select(property => $"{property.Name}: {property.Value.GetString()}")
                    .Where(part => !string.IsNullOrWhiteSpace(part));

                return string.Join(", ", parts);
            }
            catch (JsonException)
            {
                // Dữ liệu Attributes hỏng thì bỏ qua, vẫn còn tên biến thể để AI dùng
                return string.Empty;
            }
        }

        // Từ đệm tiếng Việt: không giúp nhận diện sản phẩm nên loại ra để khỏi nhiễu điểm xếp hạng
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "có", "gì", "sao", "còn", "là", "của", "cho", "với", "và", "thì", "nào", "bao", "nhiêu",
            "mình", "bạn", "ạ", "không", "ko", "cần", "muốn", "xin", "chào", "dạ", "hiện", "tại",
            "này", "đó", "về", "vẫn", "được", "ở", "mà", "hay", "cửa", "hàng", "sản", "phẩm",
            "thông", "tin", "thêm", "bao nhiêu", "giá", "màu", "sắc", "dung", "lượng", "phiên", "bản"
        };

        private static IEnumerable<string> Tokenize(string text)
        {
            return text
                .ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', ';', ':', '-', '_', '/', '\\', '?', '!', '(', ')', '\n', '\r', '\t' },
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length >= 2 && !StopWords.Contains(word));
        }

        private async Task<string> BuildProductContextAsync(string message, IReadOnlyList<ChatMessageDto> history, CancellationToken cancellationToken)
        {
            // Câu hỏi nối tiếp kiểu "còn màu gì" / "dung lượng sao" không chứa tên máy nào cả.
            // Vì vậy phải lấy từ khóa từ CẢ lịch sử hội thoại, nếu không sẽ không truy ra được sản phẩm đang bàn.
            // Từ khóa ở câu hiện tại được tính điểm nặng hơn từ khóa cũ trong lịch sử.
            var weightedKeywords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var word in Tokenize(message).Distinct())
            {
                weightedKeywords[word] = 3;
            }

            var recentHistory = (history ?? Array.Empty<ChatMessageDto>())
                .Where(item => !string.IsNullOrWhiteSpace(item.Content))
                .TakeLast(6);

            foreach (var item in recentHistory)
            {
                foreach (var word in Tokenize(item.Content).Distinct())
                {
                    if (!weightedKeywords.ContainsKey(word))
                        weightedKeywords[word] = 1;
                }
            }

            var keywords = weightedKeywords
                .OrderByDescending(pair => pair.Value)
                .Take(30)
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

            var lowerMsg = message.ToLowerInvariant();
            var wantsAccessory = lowerMsg.Contains("ốp") || lowerMsg.Contains("dán") || lowerMsg.Contains("sạc") || lowerMsg.Contains("tai nghe") || lowerMsg.Contains("phụ kiện") || lowerMsg.Contains("kính");

            var rankedProductsList = products
                .Select(product =>
                {
                    var prodNameLower = product.Name.ToLowerInvariant();
                    var catLower = product.CategoryName.ToLowerInvariant();

                    int matchScore = keywords.Count(keyword =>
                        prodNameLower.Contains(keyword.Key) ||
                        product.BrandName.ToLowerInvariant().Contains(keyword.Key) ||
                        catLower.Contains(keyword.Key));

                    if (matchScore == 0) return new { Product = product, TotalScore = 0 };

                    int priorityScore = matchScore * 10;

                    // Nếu người dùng KHÔNG chủ động hỏi phụ kiện -> Ưu tiên Điện thoại lên hàng đầu (+100 điểm)
                    bool isPhone = catLower.Contains("điện thoại") || catLower.Contains("phone") || catLower.Contains("smartphone");
                    bool isAccessoryName = prodNameLower.Contains("ốp") || prodNameLower.Contains("dán") || prodNameLower.Contains("kính") || prodNameLower.Contains("bao da") || prodNameLower.Contains("sạc");

                    if (!wantsAccessory)
                    {
                        if (isPhone && !isAccessoryName) priorityScore += 100;
                        if (isAccessoryName) priorityScore -= 50; // Trừ điểm phụ kiện khi hỏi mua máy
                    }
                    else
                    {
                        if (isAccessoryName) priorityScore += 100;
                    }

                    return new { Product = product, TotalScore = priorityScore };
                })
                .Where(item => item.TotalScore > 0)
                .OrderByDescending(item => item.TotalScore)
                .ThenByDescending(item => item.Product.BasePrice)
                .Take(15)
                .ToList();

            // Nạp biến thể (màu sắc, dung lượng, giá, tồn) cho các sản phẩm khớp nhất,
            var matched = rankedProductsList.Where(item => item.TotalScore > 0).Select(item => item.Product.Id).Take(8).ToList();
            var topProductIds = matched.Any()
                ? matched
                : rankedProductsList.Select(item => item.Product.Id).Take(3).ToList();

            var variants = await _context.ProductVariants
                .Where(variant => variant.IsActive && topProductIds.Contains(variant.ProductId))
                .Select(variant => new
                {
                    variant.ProductId,
                    variant.Name,
                    variant.Price,
                    variant.TotalStock,
                    variant.ReservedStock,
                    variant.Attributes
                })
                .ToListAsync(cancellationToken);

            var variantsByProduct = variants
                .GroupBy(variant => variant.ProductId)
                .ToDictionary(group => group.Key, group => group.Take(10).ToList());

            var rankedProducts = rankedProductsList.Select(item =>
            {
                var product = item.Product;
                var stock = product.TotalStock - product.ReservedStock;
                var line = $"- #{product.Id} [{product.CategoryName}] {product.Name} | Hãng: {product.BrandName} | Giá từ: {product.BasePrice.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))}đ | Tồn: {(stock > 0 ? "còn hàng" : "hết hàng")}";

                if (variantsByProduct.TryGetValue(product.Id, out var productVariants) && productVariants.Count > 0)
                {
                    var variantLines = productVariants.Select(variant =>
                    {
                        var variantStock = variant.TotalStock - variant.ReservedStock;
                        var attributes = DescribeAttributes(variant.Attributes);
                        var label = string.IsNullOrWhiteSpace(attributes) ? variant.Name : $"{variant.Name} ({attributes})";
                        return $"    • {label} | {variant.Price.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))}đ | Tồn: {(variantStock > 0 ? $"còn {variantStock}" : "hết hàng")}";
                    });
                    line += "\n  Phiên bản hiện có:\n" + string.Join("\n", variantLines);
                }

                return line;
            });

            // Lấy danh sách Chương trình Khuyến mãi đang Active từ DB
            var promotions = await _context.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.UtcNow)
                .Where(p => p.UsageLimit == 0 || p.UsedCount < p.UsageLimit)
                .Take(5)
                .Select(p => new
                {
                    p.Code,
                    p.DiscountType,
                    p.DiscountValue,
                    p.MinOrderAmount,
                    p.MaxDiscountAmount
                })
                .ToListAsync(cancellationToken);

            var vietnam = CultureInfo.GetCultureInfo("vi-VN");
            var activePromotions = promotions.Select(p =>
            {
                // DiscountType lưu dạng chuỗi ("percent"/"fixed"), quy về mô tả dễ hiểu cho AI
                var isPercent = !string.IsNullOrWhiteSpace(p.DiscountType) &&
                                p.DiscountType.Contains("percent", StringComparison.OrdinalIgnoreCase);
                var amount = isPercent
                    ? $"giảm {p.DiscountValue:0.##}%"
                    : $"giảm {p.DiscountValue.ToString("N0", vietnam)}đ";

                var line = $"- Mã [{p.Code}]: {amount}";
                if (p.MinOrderAmount.HasValue && p.MinOrderAmount.Value > 0)
                    line += $", áp dụng cho đơn từ {p.MinOrderAmount.Value.ToString("N0", vietnam)}đ";
                if (p.MaxDiscountAmount.HasValue && p.MaxDiscountAmount.Value > 0)
                    line += $", giảm tối đa {p.MaxDiscountAmount.Value.ToString("N0", vietnam)}đ";

                return line;
            }).ToList();

            var contextText = "--- NGỮ CẢNH SẢN PHẨM & PHỤ KIỆN TÌM THẤY ---\n" + string.Join("\n", rankedProducts);

            // Lấy danh sách các gói Bảo hành Mở rộng đang Active từ DB kèm theo quy tắc phạm vi áp dụng
            var warranties = await _context.Warranties
                .Where(w => w.IsActive)
                .Take(10)
                .Select(w => new
                {
                    w.Id,
                    w.Code,
                    w.Name,
                    w.Description,
                    w.DurationMonths,
                    w.BasePrice,
                    Rules = _context.WarrantyPackageRules
                        .Where(r => r.WarrantyId == w.Id)
                        .Select(r => new
                        {
                            BrandName = r.Brand != null ? r.Brand.Name : null,
                            CategoryName = r.Category != null ? r.Category.Name : null,
                            ProductName = r.Product != null ? r.Product.Name : null,
                            r.MinPrice,
                            r.MaxPrice
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var activeWarranties = warranties.Select(w =>
            {
                var line = $"- Tên gói: \"{w.Name}\" | Giá: {w.BasePrice.ToString("N0", vietnam)}đ | Thời hạn: {w.DurationMonths} tháng";
                if (!string.IsNullOrWhiteSpace(w.Description))
                    line += $"\n  Mô tả: {w.Description}";

                if (w.Rules != null && w.Rules.Any())
                {
                    var ruleLines = w.Rules.Select(r =>
                    {
                        var brand = !string.IsNullOrEmpty(r.BrandName) ? r.BrandName : "Tất cả thương hiệu (Không ràng buộc)";
                        var cat = !string.IsNullOrEmpty(r.CategoryName) ? r.CategoryName : "Tất cả danh mục (Không ràng buộc)";
                        var prod = !string.IsNullOrEmpty(r.ProductName) ? r.ProductName : "Tất cả sản phẩm (Không ràng buộc)";
                        var price = (r.MinPrice > 0 || r.MaxPrice.HasValue)
                            ? (r.MaxPrice.HasValue ? $"Từ {r.MinPrice.ToString("N0", vietnam)}đ đến {r.MaxPrice.Value.ToString("N0", vietnam)}đ" : $"Từ {r.MinPrice.ToString("N0", vietnam)}đ đến Không giới hạn")
                            : "Không giới hạn";

                        return $"  CHI TIẾT ÁP DỤNG: Thương hiệu áp dụng: {brand} | Danh mục áp dụng: {cat} | Sản phẩm cụ thể: {prod} | Khoảng giá máy: {price}";
                    });
                    line += "\n" + string.Join("\n", ruleLines);
                }
                else
                {
                    line += "\n  CHI TIẾT ÁP DỤNG: Thương hiệu áp dụng: Tất cả thương hiệu (Áp dụng chung) | Danh mục áp dụng: Tất cả danh mục | Sản phẩm cụ thể: Tất cả sản phẩm";
                }

                return line;
            }).ToList();

            if (activeWarranties.Any())
            {
                contextText += "\n\n--- CÁC GÓI BẢO HÀNH MỞ RỘNG CỦA PHONESHOP ---\n" + string.Join("\n", activeWarranties);
            }

            if (activePromotions.Any())
            {
                contextText += "\n\n--- MÃ GIẢM GIÁ & KHUYẾN MÃI ĐANG ÁP DỤNG ---\n" + string.Join("\n", activePromotions);
            }

            return contextText;
        }

        private static string BuildLocalDatabaseReply(string userQuery, string productContext)
        {
            if (string.IsNullOrWhiteSpace(productContext) || !productContext.Contains("--- NGỮ CẢNH SẢN PHẨM"))
            {
                return "Dạ hiện tại PhoneShop chưa tìm thấy sản phẩm khớp chính xác với câu hỏi của bạn. Bạn có thể thử tìm kiếm theo tên hãng (iPhone, Samsung, OPPO, Xiaomi...) hoặc loại sản phẩm (Điện thoại, Tai nghe, Phụ kiện) nhé!";
            }

            var lines = productContext.Split('\n')
                .Where(line => line.StartsWith("- #"))
                .Take(5)
                .ToList();

            if (!lines.Any())
            {
                return "Dạ PhoneShop đã tiếp nhận câu hỏi của bạn. Hiện tại hệ thống đang cập nhật danh mục sản phẩm, bạn có thể tham khảo trực tiếp trên thanh tìm kiếm của website ạ!";
            }

            var replyText = "Dạ PhoneShop tìm thấy các sản phẩm liên quan đến câu hỏi của bạn:\n\n";
            foreach (var line in lines)
            {
                replyText += line + "\n";
            }

            replyText += "\n💡 Bạn cần mình tư vấn thêm về thông tin cấu hình hay màu sắc nào của các sản phẩm trên không ạ?";
            return replyText;
        }
    }
}
