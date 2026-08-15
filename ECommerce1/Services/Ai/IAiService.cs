// ==========================================================================
// MODULE: IAiService.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module IAiService
// ==========================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce1.Services.Ai
{
    // [Hàm thực thi nghiệp vụ]: `Method` - Xử lý logic và luồng dữ liệu
    public interface IAiService
    {
        bool IsConfigured { get; }
        Task<string> ChatAsync(string userMessage, IReadOnlyList<ChatMessageDto> history, string productContext, CancellationToken cancellationToken = default);
        Task<ReviewModerationResult> ModerateReviewAsync(string comment, CancellationToken cancellationToken = default);
    }
}
