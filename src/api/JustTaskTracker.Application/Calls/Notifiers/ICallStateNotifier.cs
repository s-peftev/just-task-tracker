using JustTaskTracker.Domain.Calls.Notifications;

namespace JustTaskTracker.Application.Calls.Notifiers;

public interface ICallStateNotifier
{
    Task NotifyAsync(CallStateNotification notification, CancellationToken ct);

    /// <summary>
    /// AD-10: cross-page "call started" alert, pushed to specific users (by AzureAdObjectId)
    /// regardless of which board group (if any) their connection currently belongs to.
    /// </summary>
    Task NotifyCallStartedAsync(CallStartedAlert alert, IReadOnlyList<Guid> recipientAzureAdObjectIds, CancellationToken ct);
}
