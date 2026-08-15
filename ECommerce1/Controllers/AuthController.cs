// ==========================================================================
// MODULE: AuthController.cs
// MỤC ĐÍCH: API Controller xử lý Đăng ký, Đăng nhập, Refresh Token, Đổi mật khẩu và Quên mật khẩu.
// ==========================================================================
using ECommerce.Models;
using ECommerce1.DTOs.Auth;
using ECommerce1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ECommerce1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<User> _hasher;
        private readonly IConfiguration _configuration;
        private readonly ECommerce1.Services.IEmailService _emailService;

        public AuthController(ApplicationDbContext context, TokenService tokenService, IConfiguration configuration, ECommerce1.Services.IEmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _hasher = new PasswordHasher<User>();
            _configuration = configuration;
            _emailService = emailService;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        // [Hàm thực thi nghiệp vụ]: `Register` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || 
                string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Vui lòng điền đầy đủ Tên đăng nhập, Email và Mật khẩu.");
            }

            var cleanUsername = request.Username.Trim();
            var cleanEmail = request.Email.Trim().ToLower();

            if (!cleanEmail.Contains("@") || !cleanEmail.Contains("."))
            {
                return BadRequest("Địa chỉ Email không hợp lệ.");
            }

            if (request.Password.Length < 6)
            {
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");
            }

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            if (await _context.Users.AnyAsync(x => x.Username.ToLower() == cleanUsername.ToLower()))
            {
                return BadRequest("Tên đăng nhập này đã được sử dụng.");
            }

            if (await _context.Users.AnyAsync(x => x.Email.ToLower() == cleanEmail))
            {
                return BadRequest("Địa chỉ Email này đã được đăng ký tài khoản khác.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = cleanUsername,
                Email = cleanEmail,
                Role = "User",
                IsActive = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.Users.Add(user);
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Id = user.Id,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                RewardPoints = user.RewardPoints,
                AccumulatedPoints = user.AccumulatedPoints
            };

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        // [Hàm thực thi nghiệp vụ]: `Login` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == request.Username || x.Email == request.Username);

            if (user == null)
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không chính xác.");

            if (!user.IsActive)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ với nhân viên của cửa hàng hoặc qua SĐT:18001062.");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Invalid username or password");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // Sử dụng chính LoginRequest để làm object trả về
            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Id = user.Id,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                RewardPoints = user.RewardPoints,
                AccumulatedPoints = user.AccumulatedPoints
            };

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(response);
        }

        // ================= GOOGLE LOGIN =================
        [HttpPost("google-login")]
        // [Hàm thực thi nghiệp vụ]: `GoogleLogin` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var googleClientId = _configuration["Google:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest("Google Client ID is not configured.");
                }
                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { googleClientId }
                };
                
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                
                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
                if (user != null && !user.IsActive)
                {
                    // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                    return BadRequest("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                }

                if (user == null)
                {
                    // Tạo tài khoản mới cho user
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = payload.Email.Split('@')[0] + "_" + new Random().Next(1000, 9999), // Tránh trùng username
                        Email = payload.Email,
                        Role = "User",
                        IsEmailVerified = true // Google đã xác thực
                    };
                    user.PasswordHash = _hasher.HashPassword(user, Guid.NewGuid().ToString()); // Random password
                    // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                    _context.Users.Add(user);
                    // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                    await _context.SaveChangesAsync();
                }
                else if (!user.IsEmailVerified)
                {
                    user.IsEmailVerified = true;
                    await _context.SaveChangesAsync();
                }

                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
                _context.RefreshTokens.Add(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                });
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();

                // [Phản hồi API]: Trả về kết quả Ok cho phía Client
                return Ok(new LoginResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    Id = user.Id,
                    Role = user.Role,
                    IsEmailVerified = user.IsEmailVerified,
                    RewardPoints = user.RewardPoints,
                    AccumulatedPoints = user.AccumulatedPoints
                });
            }
            catch (Exception ex)
            {
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Xác thực Google thất bại: " + ex.Message);
            }
        }

        // ================= REFRESH =================
        [HttpPost("refresh")]
        // [Hàm thực thi nghiệp vụ]: `Refresh` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Refresh(TokenRequest request)
        {
            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Invalid refresh token");

            if (storedToken.User != null && !storedToken.User.IsActive)
                // [Phản hồi API]: Trả về kết quả Unauthorized cho phía Client
                return Unauthorized("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");

            var newAccessToken = _tokenService.GenerateAccessToken(storedToken.User);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            storedToken.IsRevoked = true;

            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = storedToken.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        // ================= LOGOUT =================
        [HttpPost("logout")]
        // [Hàm thực thi nghiệp vụ]: `Logout` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> Logout(TokenRequest request)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
                await _context.SaveChangesAsync();
            }

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok("Logged out");
        }
        // ================= XỬ LÝ QUÊN MẬT KHẨU =================
        [HttpPost("forgot-password")]
        // [Hàm thực thi nghiệp vụ]: `ForgotPassword` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ForgotPassword([FromBody] ECommerce1.DTOs.Auth.ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Vui lòng nhập Email hoặc Tên đăng nhập.");

            string inputVal = request.Email.Trim();
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == inputVal || u.Username == inputVal);
            if (user == null)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Không tìm thấy tài khoản với thông tin này.");

            if (string.IsNullOrWhiteSpace(user.Email))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Tài khoản chưa đăng ký Email nhận mã.");

            // Tạo mã OTP ngẫu nhiên 6 số
            var otp = new Random().Next(100000, 999999).ToString();
            
            user.ResetPasswordToken = otp;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15); // Hết hạn sau 15 phút
            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            string subject = "PhoneStore - Mã xác nhận cấp lại mật khẩu";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #1a73e8; text-align: center;'>PhoneStore</h2>
                    <h3 style='color: #333;'>Xin chào <b>{user.Username}</b>,</h3>
                    <p>Bạn đã gửi yêu cầu cấp lại mật khẩu cho tài khoản tại PhoneStore.</p>
                    <p>Vui lòng sử dụng mã xác nhận (OTP) bên dưới để tiến hành đổi mật khẩu:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='font-size: 28px; font-weight: bold; color: #1a73e8; background: #e8f0fe; padding: 10px 24px; border-radius: 6px; letter-spacing: 4px;'>{otp}</span>
                    </div>
                    <p style='color: #666; font-size: 13px;'>Mã này có hiệu lực trong vòng <b>15 phút</b>. Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'/>
                    <p style='font-size: 11px; color: #999; text-align: center;'>Trân trọng,<br/>Đội ngũ PhoneStore</p>
                </div>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Mã xác nhận (OTP) đã được gửi đến email của bạn.", email = user.Email });
        }

        // [API Endpoint POST [Route: `reset-password`]]: Tiếp nhận và xử lý yêu cầu từ Client
        [HttpPost("reset-password")]
        // [Hàm thực thi nghiệp vụ]: `ResetPassword` - Xử lý logic và luồng dữ liệu
        public async Task<IActionResult> ResetPassword([FromBody] ECommerce1.DTOs.Auth.ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Vui lòng cung cấp đầy đủ Email, Mã OTP và Mật khẩu mới.");

            string inputVal = request.Email.Trim();
            // [Truy vấn CSDL EF Core]: Đọc/Lọc dữ liệu từ SQL Server
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == inputVal || u.Username == inputVal);
            if (user == null)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Tài khoản không tồn tại.");

            if (user.ResetPasswordToken != request.Otp.Trim())
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã xác nhận (OTP) không chính xác.");

            if (!user.ResetPasswordTokenExpiry.HasValue || user.ResetPasswordTokenExpiry.Value < DateTime.UtcNow)
                // [Phản hồi API]: Trả về kết quả BadRequest cho phía Client
                return BadRequest("Mã xác nhận (OTP) đã hết hạn. Vui lòng yêu cầu gửi lại mã mới.");

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            
            // Xóa OTP sau khi sử dụng thành công
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            // [Lưu vào CSDL]: Thực thi ghi/cập nhật dữ liệu xuống CSDL SQL Server
            await _context.SaveChangesAsync();

            // [Phản hồi API]: Trả về kết quả Ok cho phía Client
            return Ok(new { message = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới." });
        }

        // ================= SEND EMAIL VERIFICATION OTP =================
        [HttpPost("send-verification-otp")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> SendVerificationOtp()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            if (user.IsEmailVerified)
                return BadRequest("Email của bạn đã được xác thực trước đó.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Tài khoản chưa cập nhật địa chỉ Email.");

            var otp = new Random().Next(100000, 999999).ToString();
            user.EmailVerificationToken = otp;
            user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            string subject = "PhoneStore - Mã xác thực Email tài khoản";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #1a73e8; text-align: center;'>PhoneStore</h2>
                    <h3 style='color: #333;'>Xin chào <b>{user.Username}</b>,</h3>
                    <p>Bạn đã yêu cầu gửi mã xác thực Email cho tài khoản tại PhoneStore.</p>
                    <p>Mã OTP xác thực email của bạn là:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='font-size: 28px; font-weight: bold; color: #34a853; background: #e6f4ea; padding: 10px 24px; border-radius: 6px; letter-spacing: 4px;'>{otp}</span>
                    </div>
                    <p style='color: #666; font-size: 13px;'>Mã này có hiệu lực trong vòng <b>15 phút</b>. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'/>
                    <p style='font-size: 11px; color: #999; text-align: center;'>Trân trọng,<br/>Đội ngũ PhoneStore</p>
                </div>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok(new { message = "Mã xác thực OTP đã được gửi tới email của bạn.", email = user.Email });
        }

        // ================= VERIFY EMAIL =================
        [HttpPost("verify-email")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> VerifyEmail([FromBody] ECommerce1.DTOs.Auth.VerifyEmailRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Otp))
                return BadRequest("Vui lòng nhập mã OTP xác thực.");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            if (user.IsEmailVerified)
                return Ok(new { message = "Email của bạn đã được xác thực trước đó.", isEmailVerified = true });

            if (user.EmailVerificationToken != request.Otp.Trim())
                return BadRequest("Mã OTP xác thực không chính xác.");

            if (!user.EmailVerificationExpiry.HasValue || user.EmailVerificationExpiry.Value < DateTime.UtcNow)
                return BadRequest("Mã OTP xác thực đã hết hạn. Vui lòng lấy mã mới.");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Xác thực email thành công!", isEmailVerified = true });
        }
    }
}
