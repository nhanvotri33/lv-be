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
                RewardPoints = user.RewardPoints
            };

            return Ok(response);
        }

        // ================= GOOGLE LOGIN =================
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var googleClientId = _configuration["Google:ClientId"] ?? "702529174883-k7q714ds1n185oaabhl85hfhhqhqg7dq.apps.googleusercontent.com";
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
                    RewardPoints = user.RewardPoints
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Không tìm thấy tài khoản với email này.");

            // Tạo mã OTP ngẫu nhiên 6 số
            var otp = new Random().Next(100000, 999999).ToString();
            
            user.ResetPasswordToken = otp;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15); // Hết hạn sau 15 phút
            await _context.SaveChangesAsync();

            string subject = "PhoneStore - Mã xác nhận cấp lại mật khẩu";
            string body = $@"
                <h3>Xin chào {user.Username},</h3>
                <p>Bạn đã yêu cầu cấp lại mật khẩu. Vui lòng sử dụng mã xác nhận (OTP) sau để đặt lại mật khẩu của bạn:</p>
                <h2 style='color: blue;'>{otp}</h2>
                <p>Mã này sẽ hết hạn trong vòng 15 phút.</p>
                <p>Nếu bạn không yêu cầu đổi mật khẩu, vui lòng bỏ qua email này.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok("Mã xác nhận (OTP) đã được gửi đến email của bạn.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ECommerce1.DTOs.Auth.ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Email không hợp lệ.");

            if (user.ResetPasswordToken != request.Otp)
                return BadRequest("Mã xác nhận không chính xác.");

            if (user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                return BadRequest("Mã xác nhận đã hết hạn. Vui lòng yêu cầu mã mới.");

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            
            // Xóa OTP sau khi sử dụng thành công
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.");
        }
    }
}