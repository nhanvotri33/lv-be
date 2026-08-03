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

        [HttpGet("product/{productId}")]
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

            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllReviewsForAdmin()
        {
            var reviews = await BuildAdminReviewsQuery().ToListAsync();
            return Ok(reviews);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId
                            && o.OrderStatusId == 4
                            && o.OrderItems.Any(oi => oi.ProductVariant.ProductId == request.ProductId));

            if (!hasPurchased)
                return BadRequest("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua và nhận hàng thành công.");

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

            if (existingReview != null)
                return BadRequest("Bạn đã đánh giá sản phẩm này rồi.");

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Số sao đánh giá phải từ 1 đến 5.");

            if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Trim().Length < 10)
                return BadRequest("Nội dung đánh giá phải có tối thiểu 10 ký tự.");

            var moderation = await _aiService.ModerateReviewAsync(request.Comment, HttpContext.RequestAborted);
            var review = new Review
            {
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Comment = System.Net.WebUtility.HtmlEncode(request.Comment.Trim()),
                CreatedAt = DateTime.UtcNow,
                IsHidden = !moderation.IsAllowed
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            if (!moderation.IsAllowed)
                return Ok("Cảm ơn bạn đã gửi đánh giá. Nội dung đang chờ quản trị viên duyệt trước khi hiển thị.");

            return Ok("Cảm ơn bạn đã đánh giá sản phẩm.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/reply")]
        public async Task<IActionResult> AdminReply(int id, [FromBody] AdminReplyRequest request)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound("Không tìm thấy bài đánh giá.");

            review.AdminReply = request.Reply;
            review.RepliedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Đã phản hồi bài đánh giá.");
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("{id}/toggle-visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound("Không tìm thấy bài đánh giá.");

            review.IsHidden = !review.IsHidden;
            await _context.SaveChangesAsync();

            string status = review.IsHidden ? "đã bị ẩn" : "đã được hiển thị lại";
            return Ok($"Bài đánh giá {status}.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllReviewsForAdminDefault()
        {
            var reviews = await BuildAdminReviewsQuery().ToListAsync();
            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound("Không tìm thấy bài đánh giá.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
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