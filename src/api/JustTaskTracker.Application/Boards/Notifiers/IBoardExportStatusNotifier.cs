using JustTaskTracker.Domain.Boards.Messaging;

namespace JustTaskTracker.Application.Boards.Notifiers;

public interface IBoardExportStatusNotifier
{
    Task NotifyStatusChangedAsync(
        BoardExportStatusChangedNotification notification,
        CancellationToken ct = default);
}
