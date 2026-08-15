// ==========================================================================
// MODULE: AiDtos.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module AiDtos
// ==========================================================================
using System.Collections.Generic;

namespace ECommerce1.Services.Ai
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto> History { get; set; } = new();
    }

    public class ChatbotResponse
    {
        public string Reply { get; set; } = string.Empty;
    }

    public class ReviewModerationResult
    {
        public bool IsAllowed { get; set; } = true;
        public string Reason { get; set; } = string.Empty;
    }
}
