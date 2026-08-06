using JustTaskTracker.Application.Assistant.Repositories;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class GetActiveOwnedBoardsCountToolHandler(IAssistantDataQueryRepository assistantDataQueryRepository)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.GetActiveOwnedBoardsCount;

    public string Description =>
        "Get how many active (non-archived) boards the current user owns. " +
        "Use for questions about owned board count or board-limit usage.";

    public BinaryData ParametersSchema => AssistantToolSchemas.EmptyObject;

    public async Task<string> ExecuteAsync(Guid currentUserId, BinaryData arguments, CancellationToken ct = default)
    {
        var count = await assistantDataQueryRepository.GetActiveOwnedBoardsCountAsync(currentUserId, ct);
        return AssistantToolJson.Serialize(new { count });
    }
}
