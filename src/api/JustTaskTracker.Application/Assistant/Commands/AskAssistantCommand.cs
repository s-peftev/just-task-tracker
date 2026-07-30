using FluentValidation;
using JustTaskTracker.Application.Assistant.Abstractions;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Assistant.DTOs;
using JustTaskTracker.Domain.Assistant.Enums;
using JustTaskTracker.Domain.Assistant.Errors;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Assistant.Commands;

public record AskAssistantCommand(string Message, IReadOnlyList<AssistantChatMessageDto> History)
    : IRequest<Result<AssistantChatReplyDto>>;

public class AskAssistantCommandHandler(
    IKnowledgeBaseSearchService knowledgeBaseSearchService,
    IAssistantCompletionService assistantCompletionService,
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IEntitlementService entitlementService,
    AssistantPromptOptions promptOptions,
    ILogger<AskAssistantCommandHandler> logger)
    : IRequestHandler<AskAssistantCommand, Result<AssistantChatReplyDto>>
{
    public async Task<Result<AssistantChatReplyDto>> Handle(AskAssistantCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result<AssistantChatReplyDto>.Failure(GeneralErrors.Unauthorized);

        IReadOnlyList<RetrievedChunk> chunks;

        try
        {
            chunks = await knowledgeBaseSearchService.SearchAsync(request.Message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Knowledge-base search failed for assistant chat.");
            return Result<AssistantChatReplyDto>.Failure(AssistantErrors.KnowledgeBaseUnavailable);
        }

        chunks = [.. chunks.Where(chunk => !string.IsNullOrWhiteSpace(chunk.Content))];

        if (chunks.Count == 0)
            return Result<AssistantChatReplyDto>.Success(new AssistantChatReplyDto(promptOptions.NoContextReply));

        var entitlements = await entitlementService.GetEntitlementsAsync(currentUserInfo.Id, ct);
        var systemPrompt = BuildSystemPrompt(
            promptOptions,
            currentUserAccessor.AppRoles,
            entitlements,
            chunks);

        IReadOnlyList<AssistantChatMessageDto> messages =
        [
            .. request.History,
            new AssistantChatMessageDto(AssistantMessageRole.User, request.Message.Trim())
        ];

        try
        {
            var answer = await assistantCompletionService.GetAnswerAsync(systemPrompt, messages, ct);

            if (string.IsNullOrWhiteSpace(answer))
                return Result<AssistantChatReplyDto>.Failure(AssistantErrors.CompletionFailed);

            return Result<AssistantChatReplyDto>.Success(new AssistantChatReplyDto(answer));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Assistant completion failed.");
            return Result<AssistantChatReplyDto>.Failure(AssistantErrors.CompletionFailed);
        }
    }

    private static string BuildSystemPrompt(
        AssistantPromptOptions promptOptions,
        IReadOnlyList<string> globalRoles,
        EntitlementDto entitlements,
        IReadOnlyList<RetrievedChunk> chunks)
    {
        var userContext = BuildUserContext(globalRoles, entitlements);
        var knowledgeContext = string.Join("\n\n", chunks.Select(chunk => chunk.Content));

        return
            $"""
            {promptOptions.SystemPrompt}

            Current user:
            {userContext}
            {promptOptions.UserContextInstruction}

            Context:
            {knowledgeContext}
            """;
    }

    private static string BuildUserContext(IReadOnlyList<string> globalRoles, EntitlementDto entitlements)
    {
        var roles = globalRoles.Count == 0
            ? "none"
            : string.Join(", ", globalRoles);

        var features = entitlements.Features.Count == 0
            ? "none"
            : string.Join(", ", entitlements.Features);

        var limits = entitlements.Limits;

        return
            $"""
            - Global roles: {roles}
            - Plan: {entitlements.PlanDisplayName} ({entitlements.PlanId})
            - Subscription status: {entitlements.Status}
            - Features: {features}
            - Limits: MaxBoards={FormatLimit(limits.MaxBoards)}, MaxColumnsPerBoard={FormatLimit(limits.MaxColumnsPerBoard)}, MaxTasksPerBoard={FormatLimit(limits.MaxTasksPerBoard)}, MaxMembersPerBoard={FormatLimit(limits.MaxMembersPerBoard)}
            """.Trim();
    }

    private static string FormatLimit(int? limit) =>
        limit is null ? "unlimited" : limit.Value.ToString();
}

public class AskAssistantCommandValidator : AbstractValidator<AskAssistantCommand>
{
    public AskAssistantCommandValidator(ValidationSettings validationSettings)
    {
        var assistantSettings = validationSettings.Assistant!;
        var maxMessageLength = assistantSettings.MaxMessageLength;
        var maxHistoryMessages = assistantSettings.MaxHistoryMessages;

        RuleFor(x => x.Message)
            .Must(message => !string.IsNullOrWhiteSpace(message))
            .WithMessage("'Message' must not be empty.")
            .MaximumLength(maxMessageLength);

        RuleFor(x => x.History)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.History)
                    .Must(history => history.Count <= maxHistoryMessages)
                    .WithMessage($"'History' must not contain more than {maxHistoryMessages} messages.");

                RuleForEach(x => x.History)
                    .ChildRules(message =>
                    {
                        message.RuleFor(x => x.Role)
                            .IsInEnum();

                        message.RuleFor(x => x.Content)
                            .Must(content => !string.IsNullOrWhiteSpace(content))
                            .WithMessage("'Content' must not be empty.")
                            .MaximumLength(maxMessageLength);
                    });
            });
    }
}
