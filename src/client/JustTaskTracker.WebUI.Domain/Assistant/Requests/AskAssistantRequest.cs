using JustTaskTracker.WebUI.Domain.Assistant;

namespace JustTaskTracker.WebUI.Domain.Assistant.Requests;

public record AskAssistantRequest(
    string Message,
    IReadOnlyList<AssistantChatMessageDto> History);
