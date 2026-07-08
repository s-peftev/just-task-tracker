using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

public interface IBoardActionSyncGuard
{
    bool TryAccept(BoardActionNotification notification, Guid? currentBoardId, Guid currentUserId);

    void MarkApplied(BoardActionNotification notification);

    void Reset();
}
