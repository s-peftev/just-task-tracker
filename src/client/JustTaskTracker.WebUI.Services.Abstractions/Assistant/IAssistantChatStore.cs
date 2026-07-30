using JustTaskTracker.WebUI.Domain.Assistant;

namespace JustTaskTracker.WebUI.Services.Abstractions.Assistant;

public interface IAssistantChatStore
{
    bool IsOpen { get; }

    bool IsSending { get; }

    string? ErrorMessage { get; }

    IReadOnlyList<AssistantChatMessageDto> Messages { get; }

    event Action? StateChanged;

    void Open();

    void Close();

    void Toggle();

    Task SendAsync(string message, CancellationToken ct = default);

    void Reset();
}
