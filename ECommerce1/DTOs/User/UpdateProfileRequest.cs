namespace ECommerce1.DTOs.User
{
    public class UpdateProfileRequest
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
