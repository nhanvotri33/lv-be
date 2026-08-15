// ==========================================================================
// MODULE: AiOptions.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module AiOptions
// ==========================================================================
namespace ECommerce1.Services.Ai
{
    public class AiOptions
    {
        public string? ApiKey { get; set; }
        public string Model { get; set; } = "gpt-4o-mini";
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public int TimeoutSeconds { get; set; } = 30;
    }
}
