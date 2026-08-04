using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Assistant.Errors;

public static class AssistantErrors
{
    public static readonly Error KnowledgeBaseUnavailable = new(
        nameof(KnowledgeBaseUnavailable),
        ErrorType.ServiceUnavailable,
        ["The knowledge base is temporarily unavailable."]);

    public static readonly Error CompletionFailed = new(
        nameof(CompletionFailed),
        ErrorType.ServiceUnavailable,
        ["The assistant failed to generate a response."]);
}
