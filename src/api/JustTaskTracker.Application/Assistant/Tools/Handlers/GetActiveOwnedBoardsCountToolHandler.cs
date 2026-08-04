using JustTaskTracker.Application.Assistant.Repositories;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class GetActiveOwnedBoardsCountToolHandler(IAssistantDataQueryRepository assistantDataQueryRepository)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.GetActiveOwnedBoardsCount;

    public async Task<string> ExecuteAsync(Guid currentUserId, CancellationToken ct = default)
    {
        var count = await assistantDataQueryRepository.GetActiveOwnedBoardsCountAsync(currentUserId, ct);
        return AssistantToolJson.Serialize(new { count });
    }
}
