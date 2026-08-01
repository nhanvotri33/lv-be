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
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Username == request.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Role = "User"
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Register success");
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == request.Username || x.Email == request.Username);

            if (user == null)
                return Unauthorized("Invalid username or password");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid username or password");

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

            // Sử dụng chính LoginRequest để làm object trả về
            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Id = user.Id,
                Role = user.Role,
                RewardPoints = user.RewardPoints,
                AccumulatedPoints = user.AccumulatedPoints
            };

            return Ok(response);
        }

        // ================= GOOGLE LOGIN =================
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var googleClientId = _configuration["Google:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    return BadRequest("Google Client ID is not configured.");
                }
                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { googleClientId }
                };
                
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
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
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

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

                return Ok(new LoginResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    Id = user.Id,
                    Role = user.Role,
                    RewardPoints = user.RewardPoints,
                    AccumulatedPoints = user.AccumulatedPoints
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Xác thực Google thất bại: " + ex.Message);
            }
        }

        // ================= REFRESH =================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenRequest request)
        {
            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var newAccessToken = _tokenService.GenerateAccessToken(storedToken.User);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            storedToken.IsRevoked = true;

            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = storedToken.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        // ================= LOGOUT =================
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(TokenRequest request)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            return Ok("Logged out");
        }
        // ================= XỬ LÝ QUÊN MẬT KHẨU =================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ECommerce1.DTOs.Auth.ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Vui lòng nhập Email hoặc Tên đăng nhập.");

            string inputVal = request.Email.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == inputVal || u.Username == inputVal);
            if (user == null)
                return BadRequest("Không tìm thấy tài khoản với thông tin này.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Tài khoản chưa đăng ký Email nhận mã.");

            // Tạo mã OTP ngẫu nhiên 6 số
            var otp = new Random().Next(100000, 999999).ToString();
            
            user.ResetPasswordToken = otp;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15); // Hết hạn sau 15 phút
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

            return Ok(new { message = "Mã xác nhận (OTP) đã được gửi đến email của bạn.", email = user.Email });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ECommerce1.DTOs.Auth.ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("Vui lòng cung cấp đầy đủ Email, Mã OTP và Mật khẩu mới.");

            string inputVal = request.Email.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == inputVal || u.Username == inputVal);
            if (user == null)
                return BadRequest("Tài khoản không tồn tại.");

            if (user.ResetPasswordToken != request.Otp.Trim())
                return BadRequest("Mã xác nhận (OTP) không chính xác.");

            if (!user.ResetPasswordTokenExpiry.HasValue || user.ResetPasswordTokenExpiry.Value < DateTime.UtcNow)
                return BadRequest("Mã xác nhận (OTP) đã hết hạn. Vui lòng yêu cầu gửi lại mã mới.");

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            
            // Xóa OTP sau khi sử dụng thành công
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới." });
        }
    }
}