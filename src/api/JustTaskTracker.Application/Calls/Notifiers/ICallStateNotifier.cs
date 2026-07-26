using JustTaskTracker.Domain.Calls.Notifications;

namespace JustTaskTracker.Application.Calls.Notifiers;

public interface ICallStateNotifier
{
    Task NotifyAsync(CallStateNotification notification, CancellationToken ct);
}
