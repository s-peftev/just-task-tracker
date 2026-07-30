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

        var completionOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = promptOptions.MaxOutputTokens,
            Temperature = promptOptions.Temperature
        };

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
