// ==========================================================================
// MODULE: UserController.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module UserController
// ==========================================================================
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
        // [Hàm thực thi nghiệp vụ]: `GetMyProfile` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            // 1. DATABASE LÀM VIỆC: Chỉ truy vấn thông tin cơ bản và 2 luồng điểm thô của User
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy người dùng.");

            if (!user.IsActive)
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");

            // 2. BACKEND LÀM VIỆC: Đóng gói dữ liệu thô vào UserResponse DTO để trả về
            // (Hạng thành viên của User sẽ được tính toán động dựa trên trường AccumulatedPoints ở phía dưới)
            var response = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
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
        // [Hàm thực thi nghiệp vụ]: `UpdateProfile` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy người dùng.");

            // 1. Cập nhật Username nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(request.Username) && request.Username.Trim() != user.Username)
            {
                var cleanUsername = request.Username.Trim();
                bool usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == cleanUsername.ToLower() && u.Id != userId);
                if (usernameExists) return BadRequest("Tên tài khoản này đã được sử dụng bởi người dùng khác.");
                user.Username = cleanUsername;
            }

            // 2. Cập nhật Email nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email.Trim().ToLower() != user.Email.ToLower())
            {
                var cleanEmail = request.Email.Trim().ToLower();
                if (!cleanEmail.Contains("@") || !cleanEmail.Contains("."))
                {
                    return BadRequest("Địa chỉ Email không hợp lệ.");
                }

                bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail && u.Id != userId);
                if (emailExists) return BadRequest("Email này đã được sử dụng bởi tài khoản khác.");

                user.Email = cleanEmail;
                user.IsEmailVerified = false; // Reset trạng thái xác thực email khi người dùng thay đổi sang email mới
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
            }

            // 3. Nếu người dùng nhập mật khẩu mới, tiến hành đổi mật khẩu
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(request.OldPassword))
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest("Vui lòng nhập mật khẩu cũ để đổi mật khẩu mới.");

                if (request.NewPassword.Length < 6)
                    return BadRequest("Mật khẩu mới phải có ít nhất 6 ký tự.");

                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ECommerce1.Models.User>();
                var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
                
                if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest("Mật khẩu hiện tại không chính xác.");

                user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            }

            user.UpdatedAt = DateTime.UtcNow;
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Cập nhật thông tin cá nhân thành công.", isEmailVerified = user.IsEmailVerified });
        }

        // ================= ĐỔI MẬT KHẨU CÁ NHÂN =================
        [HttpPut("change-password")]
        // [Hàm thực thi nghiệp vụ]: `ChangePassword` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized();

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy người dùng.");

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

            // Kiểm tra mật khẩu cũ có đúng không
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mật khẩu hiện tại không chính xác.");
            }

            // Đổi mật khẩu mới (Mã hóa trước khi lưu)
            user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Đổi mật khẩu thành công.");
        }

        // ================= XEM DANH SÁCH TẤT CẢ USER (CHỈ ADMIN) =================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `GetAllUsers` - Xử lý logic và luồng dữ liệu
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
                    IsEmailVerified = u.IsEmailVerified,
                    RewardPoints = u.RewardPoints,
                    AccumulatedPoints = u.AccumulatedPoints,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(users);
        }

        // ================= KHÓA / MỞ KHÓA TÀI KHOẢN (CHỈ ADMIN) =================
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        // [Hàm thực thi nghiệp vụ]: `ToggleUserStatus` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                // [Phản hồi API]: Trả về kết quả NotFound cho phía Client
                return NotFound("Không tìm thấy người dùng này.");

            // Không cho phép Admin tự khóa chính mình
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id.ToString() == currentUserIdString)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Bạn không thể tự khóa tài khoản của chính mình.");

            // Đảo ngược trạng thái
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            string message = user.IsActive ? "Đã MỞ KHÓA tài khoản." : "Đã KHÓA tài khoản.";
            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(message);
        }
    }
}
