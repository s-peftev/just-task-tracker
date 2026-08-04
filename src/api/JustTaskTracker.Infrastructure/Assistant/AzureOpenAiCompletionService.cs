using JustTaskTracker.Application.Assistant.Abstractions;
using JustTaskTracker.Application.Assistant.Tools;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Assistant.DTOs;
using JustTaskTracker.Domain.Assistant.Enums;
using OpenAI.Chat;

namespace JustTaskTracker.Infrastructure.Assistant;

internal class AzureOpenAiCompletionService(
    ChatClient chatClient,
    AssistantPromptOptions promptOptions,
    IAssistantToolExecutor toolExecutor,
    IEnumerable<IAssistantToolHandler> toolHandlers)
    : IAssistantCompletionService
{
    private const int MaxToolRounds = 3;

    private static readonly BinaryData EmptyObjectSchema =
        BinaryData.FromString("""{"type":"object","properties":{}}""");

    public async Task<string> GetAnswerAsync(
        string systemPrompt,
        IReadOnlyList<AssistantChatMessageDto> messages,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        var chatMessages = CreateChatMessages(systemPrompt, messages);
        var completionOptions = CreateCompletionOptions(allowTools: true);

        for (var round = 0; round < MaxToolRounds; round++)
        {
            var completion = (await chatClient.CompleteChatAsync(chatMessages, completionOptions, ct)).Value;

            if (completion.ToolCalls.Count == 0)
                return ExtractText(completion);

            chatMessages.Add(new AssistantChatMessage(completion.ToolCalls));

            foreach (var toolCall in completion.ToolCalls)
            {
                var toolResult = await toolExecutor.ExecuteAsync(toolCall.FunctionName, currentUserId, ct);
                chatMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
            }
        }

        // Model kept requesting tools; force a final textual answer.
        completionOptions.ToolChoice = ChatToolChoice.CreateNoneChoice();
        var finalCompletion = (await chatClient.CompleteChatAsync(chatMessages, completionOptions, ct)).Value;
        return ExtractText(finalCompletion);
    }

    private static List<ChatMessage> CreateChatMessages(string systemPrompt, IReadOnlyList<AssistantChatMessageDto> messages)
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

        return chatMessages;
    }

    private ChatCompletionOptions CreateCompletionOptions(bool allowTools)
    {
#pragma warning disable OPENAI001 // ReasoningEffortLevel is experimental in OpenAI SDK.
        var completionOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = promptOptions.MaxOutputTokens
        };

        if (promptOptions.Temperature is { } temperature)
            completionOptions.Temperature = temperature;

        if (!string.IsNullOrWhiteSpace(promptOptions.ReasoningEffort))
            completionOptions.ReasoningEffortLevel = new ChatReasoningEffortLevel(promptOptions.ReasoningEffort);
#pragma warning restore OPENAI001

        if (allowTools)
            AddAssistantTools(completionOptions);

        return completionOptions;
    }

    private void AddAssistantTools(ChatCompletionOptions completionOptions)
    {
        foreach (var handler in toolHandlers)
        {
            completionOptions.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: handler.ToolName,
                functionDescription: handler.Description,
                functionParameters: EmptyObjectSchema));
        }
    }

    private static string ExtractText(ChatCompletion completion)
    {
        if (completion.Content is null || completion.Content.Count == 0)
            return string.Empty;

        return string.Concat(completion.Content.Select(part => part.Text));
    }
}
