// ==========================================================================
// MODULE: ErrorViewModel.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module ErrorViewModel
// ==========================================================================
namespace ECommerce.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
