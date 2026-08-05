using System.Text.Json;
using JustTaskTracker.Application.Assistant.Repositories;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class GetAssignedTasksToolHandler(IAssistantDataQueryRepository assistantDataQueryRepository)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.GetAssignedTasks;

    public string Description =>
        "Returns tasks assigned to the current user on one active board: taskId, title, and createdAtUtc. " +
        "Requires boardId from ListMyActiveBoards. " +
        "Use whenever you need the requester's assigned tasks on a board (including counting or filtering by createdAtUtc from the list). " +
        "Do not invent tasks; do not show raw ids to the user unless asked.";

    public BinaryData ParametersSchema => BinaryData.FromString(
        """
        {
          "type": "object",
          "properties": {
            "boardId": {
              "type": "string",
              "description": "Board id (GUID) from ListMyActiveBoards."
            }
          },
          "required": ["boardId"],
          "additionalProperties": false
        }
        """);

    public async Task<string> ExecuteAsync(Guid currentUserId, BinaryData arguments, CancellationToken ct)
    {
        GetAssignedTasksArgs? args;
        try
        {
            args = JsonSerializer.Deserialize<GetAssignedTasksArgs>(arguments, AssistantToolJson.Options);
        }
        catch (JsonException)
        {
            return AssistantToolJson.Error("Invalid tool arguments.");
        }

        if (args is null || !Guid.TryParse(args.BoardId, out var boardId))
            return AssistantToolJson.Error("boardId must be a valid GUID from ListMyActiveBoards.");

        var boards = await assistantDataQueryRepository.GetMyActiveBoardsAsync(currentUserId, ct);
        if (!boards.Any(board => board.BoardId == boardId))
        {
            return AssistantToolJson.Error(
                "Board was not found among the user's active boards (missing, archived, or not a member)." +
                "Tell the user you could not access that board. Do not invent or reuse an invalid boardId.");
        }

        var tasks = await assistantDataQueryRepository.GetAssignedTasksAsync(currentUserId, boardId, ct);

        return AssistantToolJson.Serialize(new
        {
            boardId,
            tasks = tasks.Select(task => new
            {
                taskId = task.TaskId,
                title = task.Title,
                createdAtUtc = task.CreatedAtUtc
            })
        });
    }

    private record GetAssignedTasksArgs(string BoardId);
}