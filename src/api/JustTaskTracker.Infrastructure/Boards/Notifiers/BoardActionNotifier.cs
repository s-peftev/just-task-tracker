using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.Infrastructure.Boards.Hubs;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace JustTaskTracker.Infrastructure.Boards.Notifiers;

public class BoardActionNotifier(IHubContext<BoardActionsHub> hubContext) : IBoardActionNotifier
{
    public Task NotifyAsync(BoardActionNotification notification, CancellationToken ct) =>
        hubContext.Clients
            .Group(HubGroupNames.BoardActions.Get(notification.BoardId))
            .SendAsync(BoardActionsHubEvents.BoardChanged, notification, ct);
}
