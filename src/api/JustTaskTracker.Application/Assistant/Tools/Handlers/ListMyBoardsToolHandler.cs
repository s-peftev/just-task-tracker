using JustTaskTracker.Application.Assistant.Repositories;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class ListMyBoardsToolHandler(IAssistantDataQueryRepository assistantDataQueryRepository)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.ListMyBoards;

    public string Description =>
        "Returns the current user's boards they belong to (active and archived): boardId, title, and isArchived for each. " +
        "Use whenever you need the requester's board list or a reliable boardId for another tool. " +
        "Match boards by title yourself; never invent board ids.";

    public BinaryData ParametersSchema => AssistantToolSchemas.EmptyObject;

    public async Task<string> ExecuteAsync(Guid currentUserId, BinaryData arguments, CancellationToken ct)
    {
        var boards = await assistantDataQueryRepository.GetMyBoardsAsync(currentUserId, ct);

        return AssistantToolJson.Serialize(new
        {
            boards = boards.Select(board => new
            {
                boardId = board.BoardId,
                title = board.BoardName,
                isArchived = board.IsArchived
            })
        });
    }
}
