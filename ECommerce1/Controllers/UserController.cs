using ECommerce.Models;
using ECommerce1.Models;
using ECommerce1.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Phải đăng nhập mới được dùng các API này
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= LẤY THÔNG TIN CÁ NHÂN CỦA MÌNH =================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // 1. DATABASE LÀM VIỆC: Chỉ truy vấn thông tin cơ bản và 2 luồng điểm thô của User
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // 2. BACKEND LÀM VIỆC: Đóng gói dữ liệu thô vào UserResponse DTO để trả về
            // (Hạng thành viên của User sẽ được tính toán động dựa trên trường AccumulatedPoints ở phía dưới)
            var response = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                RewardPoints = user.RewardPoints,           // Điểm khả dụng để tiêu dùng
                AccumulatedPoints = user.AccumulatedPoints, // Điểm tích lũy trọn đời xét hạng
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            // 3. FRONTEND NHẬN VIỆC: Frontend gọi API này để lấy thông tin điểm thô và tự động map thành Rank tương ứng (Đồng/Bạc/Vàng) kèm style màu sắc
            return Ok(response);
        }

        // ================= CẬP NHẬT THÔNG TIN CÁ NHÂN (VÀ MẬT KHẨU) =================
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // Kiểm tra trùng Email nếu có thay đổi Email
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId);
                if (emailExists) return BadRequest("Email này đã được sử dụng bởi tài khoản khác.");
                user.Email = request.Email;
            }

            // Nếu người dùng nhập mật khẩu mới, tiến hành đổi mật khẩu
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (string.IsNullOrEmpty(request.OldPassword))
                    return BadRequest("Vui lòng nhập mật khẩu cũ để đổi mật khẩu mới.");

                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ECommerce1.Models.User>();
                var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
                
                if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                    return BadRequest("Mật khẩu hiện tại không chính xác.");

                user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật thông tin cá nhân thành công.");
        }

        // ================= ĐỔI MẬT KHẨU CÁ NHÂN =================
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

            // Kiểm tra mật khẩu cũ có đúng không
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
            {
                return BadRequest("Mật khẩu hiện tại không chính xác.");
            }

            // Đổi mật khẩu mới (Mã hóa trước khi lưu)
            user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Đổi mật khẩu thành công.");
        }

        // ================= XEM DANH SÁCH TẤT CẢ USER (CHỈ ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    RewardPoints = u.RewardPoints,
                    AccumulatedPoints = u.AccumulatedPoints,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // ================= KHÓA / MỞ KHÓA TÀI KHOẢN (CHỈ ADMIN) =================
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Không tìm thấy người dùng này.");

            // Không cho phép Admin tự khóa chính mình
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id.ToString() == currentUserIdString)
                return BadRequest("Bạn không thể tự khóa tài khoản của chính mình.");

            // Đảo ngược trạng thái
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            string message = user.IsActive ? "Đã MỞ KHÓA tài khoản." : "Đã KHÓA tài khoản.";
            return Ok(message);
        }
    }
}
