using JustTaskTracker.Application.Boards.Commands.Hubs.BoardExportStatus;
using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Hubs;

public static class BoardExportHubEvents
{
    public const string ExportStatusChanged = "BoardExportStatusChanged";

    public const string ReExportStatusChanged = "BoardReExportStatusChanged";
}

[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class BoardExportStatusHub(
    ISender sender,
    ILogger<BoardExportStatusHub> logger) : Hub
{
    public async Task SubscribeAsync(IReadOnlyList<Guid> boardIds)
    {
        var ct = Context.ConnectionAborted;

        var result = await sender.Send(new SubscribeBoardExportStatusCommand(boardIds), ct);

        if (!result.IsSuccess)
            throw ToHubException(result);

        var subscribableBoardIds = result.Value;

        await Task.WhenAll(subscribableBoardIds.Select(boardId =>
            Groups.AddToGroupAsync(Context.ConnectionId, HubGroupNames.BoardExportStatus.Get(boardId), ct)));

        logger.LogDebug(
            "Subscribed connection {ConnectionId} to {BoardCount} board export groups.",
            Context.ConnectionId,
            subscribableBoardIds.Count);
    }

    public async Task UnsubscribeAsync(IReadOnlyList<Guid> boardIds)
    {
        var ct = Context.ConnectionAborted;

        var distinctBoardIds = boardIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (distinctBoardIds.Count == 0)
            return;

        await Task.WhenAll(distinctBoardIds.Select(boardId =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, HubGroupNames.BoardExportStatus.Get(boardId), ct)));

        logger.LogDebug(
            "Unsubscribed connection {ConnectionId} from {BoardCount} board export groups.",
            Context.ConnectionId,
            distinctBoardIds.Count);
    }

    private static HubException ToHubException(Result result)
    {
        var message = result.Error.Details?.FirstOrDefault()
            ?? result.Error.Code;

        return new HubException(message);
    }
}
