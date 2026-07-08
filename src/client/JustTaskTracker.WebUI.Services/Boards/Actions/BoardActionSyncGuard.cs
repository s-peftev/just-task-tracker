using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Actions;

internal sealed class BoardActionSyncGuard : IBoardActionSyncGuard
{
    private readonly Dictionary<string, DateTime> _lastAppliedAtUtc = [];

    public bool TryAccept(BoardActionNotification notification, Guid? currentBoardId, Guid currentUserId)
    {
        if (currentBoardId is not { } boardId || boardId != notification.BoardId)
            return false;

        if (currentUserId != Guid.Empty && notification.ActorUserId == currentUserId)
            return false;

        var syncKey = BoardActionSyncKey.Resolve(notification);

        if (_lastAppliedAtUtc.TryGetValue(syncKey, out var lastAppliedAtUtc)
            && notification.OccurredAtUtc <= lastAppliedAtUtc)
        {
            return false;
        }

        return true;
    }

    public void MarkApplied(BoardActionNotification notification)
    {
        var syncKey = BoardActionSyncKey.Resolve(notification);
        _lastAppliedAtUtc[syncKey] = notification.OccurredAtUtc;
    }

    public void Reset() => _lastAppliedAtUtc.Clear();
}
