using JustTaskTracker.Domain.Boards.Messaging;

namespace JustTaskTracker.Application.Boards.Notifiers;

public interface IBoardExportStatusNotifier
{
    Task NotifyExportStatusChangedAsync(BoardExportStatusChangedNotification notification, CancellationToken ct = default);

    Task NotifyReExportStatusChangedAsync(BoardExportStatusChangedNotification notification, CancellationToken ct = default);
}
