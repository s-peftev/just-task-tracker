using JustTaskTracker.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.Application.Boards.Notifiers;

public interface IBoardActionNotifier
{
    Task NotifyAsync(BoardActionNotification notification, CancellationToken ct);
}
