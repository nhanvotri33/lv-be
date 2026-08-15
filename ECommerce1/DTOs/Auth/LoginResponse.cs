// ==========================================================================
// MODULE: LoginResponse.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module LoginResponse
// ==========================================================================
namespace ECommerce1.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public Guid Id { get; set; }
        public string Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public int RewardPoints { get; set; }
        public int AccumulatedPoints { get; set; }
    }
}
