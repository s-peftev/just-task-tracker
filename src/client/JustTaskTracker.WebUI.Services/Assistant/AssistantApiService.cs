using JustTaskTracker.WebUI.Domain.Assistant;
using JustTaskTracker.WebUI.Domain.Assistant.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Assistant;
using JustTaskTracker.WebUI.Services.Api;

namespace JustTaskTracker.WebUI.Services.Assistant;

internal sealed class AssistantApiService(IAssistantApi api) : IAssistantApiService
{
    public async Task<AssistantChatReplyDto> AskAsync(AskAssistantRequest request, CancellationToken ct = default)
    {
        var response = await api.ChatAsync(request, ct);

        return ApiResponseGuard.Unwrap(response);
    }
}
