using System.Text.Json;
using JustTaskTracker.Application.Assistant.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class GetBoardContextToolHandler(
    IAssistantDataQueryRepository assistantDataQueryRepository,
    IEntitlementService entitlementService)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.GetBoardContext;

    public string Description =>
        "Returns the requester's live context for one board they belong to: title, archive state, their role, " +
        "usage vs the board owner's plan limits, and ownerPlanId. " +
        "Requires boardId from ListMyBoards. Use whenever answering questions about this board for the current user.";

    public BinaryData ParametersSchema => BinaryData.FromString(
        """
        {
          "type": "object",
          "properties": {
            "boardId": {
              "type": "string",
              "description": "Board id (GUID) from ListMyBoards."
            }
          },
          "required": ["boardId"],
          "additionalProperties": false
        }
        """);

    public async Task<string> ExecuteAsync(Guid currentUserId, BinaryData arguments, CancellationToken ct)
    {
        GetBoardContextArgs? args;
        try
        {
            args = JsonSerializer.Deserialize<GetBoardContextArgs>(arguments, AssistantToolJson.Options);
        }
        catch (JsonException)
        {
            return AssistantToolJson.Error("Invalid tool arguments.");
        }

        if (args is null || !Guid.TryParse(args.BoardId, out var boardId))
            return AssistantToolJson.Error("boardId must be a valid GUID from ListMyBoards.");

        var context = await assistantDataQueryRepository.GetBoardContextAsync(currentUserId, boardId, ct);
        if (context is null)
        {
            return AssistantToolJson.Error(
                "Board was not found or you are not a member. " +
                "Tell the user you could not access that board. Do not invent or reuse an invalid boardId.");
        }

        if (!Enum.IsDefined(typeof(BoardMemberRole), context.MemberRole))
            return AssistantToolJson.Error("Board membership role is invalid.");

        var role = (BoardMemberRole)context.MemberRole;
        var entitlements = await entitlementService.GetEntitlementsAsync(context.OwnerUserId, ct);
        var limits = entitlements.Limits;

        return AssistantToolJson.Serialize(new
        {
            boardId = context.BoardId,
            title = context.BoardName,
            isArchived = context.IsArchived,
            requesterRole = role.ToString(),
            ownerPlanId = entitlements.PlanId,
            usage = new
            {
                tasks = ToUsage(context.TaskCount, limits.MaxTasksPerBoard),
                columns = ToUsage(context.ColumnCount, limits.MaxColumnsPerBoard),
                members = ToUsage(context.MemberCount, limits.MaxMembersPerBoard)
            }
        });
    }

    private static object ToUsage(int current, int? max) =>
        new
        {
            current,
            max,
            limitReached = max is { } limit && current >= limit
        };

    private record GetBoardContextArgs(string BoardId);
}
