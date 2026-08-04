using JustTaskTracker.Domain.Assistant.DTOs;

namespace JustTaskTracker.Application.Assistant.Abstractions;

public interface IAssistantCompletionService
{
    /// <summary>
    /// Generates a single assistant reply from a system prompt and conversation messages.
    /// </summary>
    /// <param name="systemPrompt">
    /// System instructions, including any retrieved knowledge-base context.
    /// </param>
    /// <param name="messages">Prior turns plus the current user message (User/Assistant roles only).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> GetAnswerAsync(string systemPrompt, IReadOnlyList<AssistantChatMessageDto> messages, CancellationToken ct = default);
}
