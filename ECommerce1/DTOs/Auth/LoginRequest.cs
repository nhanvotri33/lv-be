using System;

namespace ECommerce1.DTOs.Auth
{
    public class LoginRequest
    {
        // --- Dữ liệu Client gửi lên ---
        public string Username { get; set; }
        public string Password { get; set; }

    }
}