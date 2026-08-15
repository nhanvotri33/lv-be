// ==========================================================================
// MODULE: UpdateProfileRequest.cs
// MỤC ĐÍCH: DTO tiếp nhận thông tin cập nhật profile người dùng
// ==========================================================================
namespace ECommerce1.DTOs.User
{
    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
