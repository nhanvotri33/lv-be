// ==========================================================================
// MODULE: ReviewController.cs
// MỤC ĐÍCH: API Controller quản lý bài đánh giá sản phẩm, kiểm duyệt tự động bằng AI/Bộ lọc từ cấm và hiển thị phản hồi từ Admin.
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.Review;
using ECommerce1.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public ReviewController(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        // [API Endpoint GET [Route: `product/{productId}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("product/{productId}")]
        // [Hàm thực thi nghiệp vụ]: `GetProductReviews` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && !r.IsHidden)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    Username = r.User.Username,
                    r.AdminReply,
                    r.RepliedAt
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        // [API Endpoint GET [Route: `admin/all`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet("admin/all")]
        // [Hàm thực thi nghiệp vụ]: `GetAllReviewsForAdmin` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllReviewsForAdmin()
        {
            var reviews = await BuildAdminReviewsQuery().ToListAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(reviews);
        }

        /// <summary>
        /// LUỒNG XỬ LÝ ĐÁNH GIÁ SẢN PHẨM:
        /// 1. Kiểm tra xác thực người dùng (JWT Token).
        /// 2. Kiểm tra điều kiện mua hàng: Khách phải có đơn hàng hoàn thành (OrderStatusId = 4) chứa sản phẩm này.
        /// 3. Kiểm tra trùng lặp: Mỗi khách hàng chỉ được đánh giá 1 lần/sản phẩm.
        /// 4. Kiểm tra hợp lệ: Số sao (1-5), nội dung (>= 10 ký tự).
        /// 5. KIỂM DUYỆT NỘI DUNG (Hệ thống lai AI + Bộ lọc từ thô tục):
        ///    - Nếu phát hiện từ thô tục / vi phạm: IsAllowed = false => IsHidden = true (Tự động chuyển vào trạng thái CHỜ DUYỆT trong Admin).
        ///    - Nếu bài viết sạch sẽ / hợp lệ: IsAllowed = true => IsHidden = false (Tự động ĐÃ DUYỆT và hiển thị ngay lên sản phẩm).
        /// </summary>
        [Authorize]
        // [API Endpoint POST]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPost]
        // [Hàm thực thi nghiệp vụ]: `CreateReview` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            // Bước 1: Kiểm tra xem user đã mua và nhận hàng thành công sản phẩm này chưa
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId
                            && o.OrderStatusId == 4
                            && o.OrderItems.Any(oi => oi.ProductVariant.ProductId == request.ProductId));

            if (!hasPurchased)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua và nhận hàng thành công.");

            // Bước 2: Kiểm tra xem user đã từng đánh giá sản phẩm này chưa
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

            if (existingReview != null)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Bạn đã đánh giá sản phẩm này rồi.");

            // Bước 3: Validate dữ liệu đầu vào
            if (request.Rating < 1 || request.Rating > 5)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Số sao đánh giá phải từ 1 đến 5.");

            if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Trim().Length < 10)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Nội dung đánh giá phải có tối thiểu 10 ký tự.");

            // Bước 4: Gọi bộ kiểm duyệt (Lọc từ cấm local + OpenAI AI Moderation)
            var moderation = await _aiService.ModerateReviewAsync(request.Comment, HttpContext.RequestAborted);
            
            // Bước 5: QUY TẮC DUYỆT ẨN/HIỆN:
            // - NẾU Rating <= 3 sao (1, 2, 3 sao) HOẶC Nội dung bị vi phạm (!moderation.IsAllowed):
            //   => IsHidden = true (Bắt buộc chuyển vào danh sách CHỜ DUYỆT trong Admin).
            // - NẾU Rating 4-5 sao VÀ Nội dung sạch sẽ hợp lệ (moderation.IsAllowed = true):
            //   => IsHidden = false (Tự động ĐÃ DUYỆT và đăng lên sản phẩm công khai ngay lập tức).
            bool requiresAdminApproval = request.Rating <= 3 || !moderation.IsAllowed;

            var review = new Review
            {
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Comment = System.Net.WebUtility.HtmlEncode(request.Comment.Trim()),
                CreatedAt = DateTime.UtcNow,
                IsHidden = requiresAdminApproval
            };

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Reviews.Add(review);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            if (requiresAdminApproval)
                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok("Cảm ơn bạn đã gửi đánh giá. Nội dung đang chờ quản trị viên duyệt trước khi hiển thị.");

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Cảm ơn bạn đã đánh giá sản phẩm.");
        }

        [Authorize(Roles = "Admin")]
        // [API Endpoint PUT [Route: `{id}/reply`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPut("{id}/reply")]
        // [Hàm thực thi nghiệp vụ]: `AdminReply` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> AdminReply(int id, [FromBody] AdminReplyRequest request)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy bài đánh giá.");

            review.AdminReply = request.Reply;
            review.RepliedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Đã phản hồi bài đánh giá.");
        }

        [Authorize(Roles = "Admin")]
        // [API Endpoint PUT [Route: `{id}/toggle-visibility`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPut("{id}/toggle-visibility")]
        // [Hàm thực thi nghiệp vụ]: `ToggleVisibility` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy bài đánh giá.");

            review.IsHidden = !review.IsHidden;
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            string status = review.IsHidden ? "đã bị ẩn" : "đã được hiển thị lại";
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok($"Bài đánh giá {status}.");
        }

        [Authorize(Roles = "Admin")]
        // [API Endpoint GET]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpGet]
        // [Hàm thực thi nghiệp vụ]: `GetAllReviewsForAdminDefault` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetAllReviewsForAdminDefault()
        {
            var reviews = await BuildAdminReviewsQuery().ToListAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        // [API Endpoint DELETE [Route: `{id}`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpDelete("{id}")]
        // [Hàm thực thi nghiệp vụ]: `DeleteReview` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> DeleteReview(int id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy bài đánh giá.");

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Reviews.Remove(review);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Xóa bài đánh giá thành công.");
        }

        private IQueryable<object> BuildAdminReviewsQuery()
        {
            return _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    Username = r.User.Username,
                    ProductName = r.Product.Name,
                    r.AdminReply,
                    r.RepliedAt,
                    r.IsHidden
                });
        }
    }
}
