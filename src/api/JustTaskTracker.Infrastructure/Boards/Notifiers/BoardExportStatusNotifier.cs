using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Domain.Boards.Messaging;
using JustTaskTracker.Infrastructure.Boards.Hubs;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace JustTaskTracker.Infrastructure.Boards.Notifiers;

internal class BoardExportStatusNotifier(
    IHubContext<BoardExportStatusHub> hubContext) : IBoardExportStatusNotifier
{
    public Task NotifyStatusChangedAsync(
        BoardExportStatusChangedNotification notification,
        CancellationToken ct = default) =>
        hubContext.Clients
            .Group(HubGroupNames.BoardExportStatus.Get(notification.BoardId))
            .SendAsync(BoardExportHubEvents.StatusChanged, notification, ct);
}
