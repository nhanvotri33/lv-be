using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce1.Services.Ai
{
    public interface IAiService
    {
        bool IsConfigured { get; }
        Task<string> ChatAsync(string userMessage, IReadOnlyList<ChatMessageDto> history, string productContext, CancellationToken cancellationToken = default);
        Task<ReviewModerationResult> ModerateReviewAsync(string comment, CancellationToken cancellationToken = default);
    }
}
