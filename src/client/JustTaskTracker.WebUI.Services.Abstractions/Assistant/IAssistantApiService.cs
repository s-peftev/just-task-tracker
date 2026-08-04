using JustTaskTracker.WebUI.Domain.Assistant;
using JustTaskTracker.WebUI.Domain.Assistant.Requests;

namespace JustTaskTracker.WebUI.Services.Abstractions.Assistant;

public interface IAssistantApiService
{
    Task<AssistantChatReplyDto> AskAsync(AskAssistantRequest request, CancellationToken ct = default);
}
