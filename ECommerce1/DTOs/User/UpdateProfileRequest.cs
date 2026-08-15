// ==========================================================================
// MODULE: UpdateProfileRequest.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module UpdateProfileRequest
// ==========================================================================
namespace ECommerce1.DTOs.User
{
    public class UpdateProfileRequest
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
