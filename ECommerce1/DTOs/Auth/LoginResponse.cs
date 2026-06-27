namespace ECommerce1.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public Guid Id { get; set; }
        public string Role { get; set; }
        public int RewardPoints { get; set; }
    }
}
