using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Commands.Hubs;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Hubs;

public static class BoardActionsHubEvents
{
    public const string BoardChanged = "BoardChanged";
}

public class BoardActionsHub(
    ISender sender,
    ICurrentUserContext currentUserContext,
    ILogger<BoardExportStatusHub> logger) : Hub
{
    public async Task SubscribeAsync(Guid boardId)
    {
        var ct = Context.ConnectionAborted;

        currentUserContext.User = Context.User;

        var result = await sender.Send(new SubscribeBoardActionsCommand(boardId), ct);

        if (!result.IsSuccess)
        {
            var message = result.Error.Details?.FirstOrDefault()
            ?? result.Error.Code;

            throw new HubException(message);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroupNames.BoardActions.Get(boardId), ct);

        logger.LogDebug(
            "Subscribed connection {ConnectionId} to BoardActions group.",
            Context.ConnectionId);
    }

    public async Task UnsubscribeAsync(Guid boardId)
    {
        var ct = Context.ConnectionAborted;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, HubGroupNames.BoardActions.Get(boardId), ct);

        logger.LogDebug(
            "Unsubscribed connection {ConnectionId} from BoardActions group.",
            Context.ConnectionId);
    }
}
