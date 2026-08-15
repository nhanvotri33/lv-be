// ==========================================================================
// MODULE: RegisterRequest.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module RegisterRequest
// ==========================================================================
﻿namespace ECommerce1.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
