using JustTaskTracker.Application.Assistant.Abstractions;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Assistant.DTOs;
using JustTaskTracker.Domain.Assistant.Enums;
using OpenAI.Chat;

namespace JustTaskTracker.Infrastructure.Assistant;

internal class AzureOpenAiCompletionService(ChatClient chatClient, AssistantPromptOptions promptOptions) : IAssistantCompletionService
{
    public async Task<string> GetAnswerAsync(string systemPrompt, IReadOnlyList<AssistantChatMessageDto> messages, CancellationToken ct = default)
    {
        var chatMessages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

        foreach (var message in messages)
        {
            chatMessages.Add(message.Role switch
            {
                AssistantMessageRole.User => new UserChatMessage(message.Content),
                AssistantMessageRole.Assistant => new AssistantChatMessage(message.Content),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(messages),
                    message.Role,
                    $"Unsupported assistant message role '{message.Role}'.")
            });
        }

#pragma warning disable OPENAI001 // ReasoningEffortLevel is experimental in OpenAI SDK.
        var completionOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = promptOptions.MaxOutputTokens
        };

        // gpt-5-mini and similar models reject non-default temperature; only set when configured.
        if (promptOptions.Temperature is { } temperature)
            completionOptions.Temperature = temperature;

        if (!string.IsNullOrWhiteSpace(promptOptions.ReasoningEffort))
            completionOptions.ReasoningEffortLevel = new ChatReasoningEffortLevel(promptOptions.ReasoningEffort);
#pragma warning restore OPENAI001

        // Web search is opt-in via WebSearchOptions; leave unset so the model cannot browse the web.

        var completion = await chatClient.CompleteChatAsync(chatMessages, completionOptions, ct);

        return ExtractText(completion.Value);
    }

    private static string ExtractText(ChatCompletion completion)
    {
        if (completion.Content is null || completion.Content.Count == 0)
            return string.Empty;

        return string.Concat(completion.Content.Select(part => part.Text));
    }
}
