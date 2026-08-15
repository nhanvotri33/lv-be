// ==========================================================================
// MODULE: VerifyEmailRequest.cs
// MỤC ĐÍCH: DTO tiếp nhận dữ liệu mã OTP xác thực email từ Client
// ==========================================================================
namespace ECommerce1.DTOs.Auth
{
    public class VerifyEmailRequest
    {
        public string Otp { get; set; }
    }
}
