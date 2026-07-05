using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Domain.Boards.Messaging;
using JustTaskTracker.Infrastructure.Boards.Hubs;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace JustTaskTracker.Infrastructure.Boards.Notifiers;

internal class BoardExportStatusNotifier(
    IHubContext<BoardExportStatusHub> hubContext) : IBoardExportStatusNotifier
{
    public Task NotifyExportStatusChangedAsync(BoardExportStatusChangedNotification notification, CancellationToken ct = default) =>
        SendAsync(BoardExportHubEvents.ExportStatusChanged, notification, ct);

    public Task NotifyReExportStatusChangedAsync(BoardExportStatusChangedNotification notification, CancellationToken ct = default) =>
        SendAsync(BoardExportHubEvents.ReExportStatusChanged, notification, ct);

    private Task SendAsync(string eventName, BoardExportStatusChangedNotification notification, CancellationToken ct) =>
        hubContext.Clients
            .Group(HubGroupNames.BoardExportStatus.Get(notification.BoardId))
            .SendAsync(eventName, notification, ct);
}
