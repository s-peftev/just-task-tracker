using JustTaskTracker.Domain.Assistant.DTOs;

namespace JustTaskTracker.Application.Assistant.Abstractions;

public interface IAssistantCompletionService
{
    /// <summary>
    /// Generates an assistant reply, running whitelisted tool calls when the model requests them.
    /// </summary>
    /// <param name="systemPrompt">System instructions, including any retrieved knowledge-base context.</param>
    /// <param name="messages">Prior turns plus the current user message (User/Assistant roles only).</param>
    /// <param name="currentUserId">Authenticated app user id for tool execution.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> GetAnswerAsync(string systemPrompt, IReadOnlyList<AssistantChatMessageDto> messages, Guid currentUserId, CancellationToken ct = default);
}
