using JustTaskTracker.WebUI.Domain.Assistant;
using JustTaskTracker.WebUI.Domain.Assistant.Requests;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IAssistantApi
{
    [Post("/api/assistant/chat")]
    Task<IApiResponse<ApiEnvelope<AssistantChatReplyDto>>> ChatAsync(
        [Body] AskAssistantRequest request,
        CancellationToken ct = default);
}
