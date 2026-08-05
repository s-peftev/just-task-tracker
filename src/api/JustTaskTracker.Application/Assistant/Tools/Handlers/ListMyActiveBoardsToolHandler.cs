using JustTaskTracker.Application.Assistant.Repositories;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class ListMyActiveBoardsToolHandler(IAssistantDataQueryRepository assistantDataQueryRepository)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.ListMyActiveBoards;

    public string Description =>
        "Returns the current user's active (non-archived) boards they belong to: boardId and title for each. " +
        "Use whenever you need the requester's board list or a reliable boardId for another tool. " +
        "Match boards by title yourself; never invent board ids.";

    public BinaryData ParametersSchema => AssistantToolSchemas.EmptyObject;

    public async Task<string> ExecuteAsync(Guid currentUserId, BinaryData arguments, CancellationToken ct = default)
    {
        var boards = await assistantDataQueryRepository.GetMyActiveBoardsAsync(currentUserId, ct);

        return AssistantToolJson.Serialize(new
        {
            boards = boards.Select(board => new
            {
                boardId = board.BoardId,
                title = board.BoardName
            })
        });
    }
}
