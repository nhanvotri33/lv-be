// ==========================================================================
// MODULE: ChangePasswordRequest.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module ChangePasswordRequest
// ==========================================================================
namespace ECommerce1.DTOs.User
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
