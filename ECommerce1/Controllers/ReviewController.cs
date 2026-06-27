using ECommerce.Models;
using ECommerce1.DTOs.Review;
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

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= GET REVIEWS FOR PRODUCT (PUBLIC) =================
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

        // ================= GET ALL REVIEWS (ADMIN ONLY) =================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllReviewsForAdmin()
        {
            var reviews = await _context.Reviews
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
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // ================= CREATE A REVIEW (USER ONLY) =================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // 1. Kiểm tra xem người dùng đã mua sản phẩm này chưa và đơn hàng đã hoàn thành chưa?
            // (OrderStatusId == 4 nghĩa là Completed / Đã giao)
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId 
                            && o.OrderStatusId == 4 
                            && o.OrderItems.Any(oi => oi.ProductVariant.ProductId == request.ProductId));

            if (!hasPurchased)
                return BadRequest("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua và nhận hàng thành công.");

            // 2. Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa? (Tùy chọn: chỉ cho phép 1 review/user/product)
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

            if (existingReview != null)
                return BadRequest("Bạn đã đánh giá sản phẩm này rồi.");

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Số sao đánh giá phải từ 1 đến 5.");

            var review = new Review
            {
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok("Cảm ơn bạn đã đánh giá sản phẩm.");
        }

        // ================= ADMIN REPLY TO REVIEW (ADMIN ONLY) =================
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

        // ================= ADMIN TOGGLE VISIBILITY (ADMIN ONLY) =================
        [Authorize(Roles = "Admin")]
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
    }
}
