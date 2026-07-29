using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Services.Abstractions.Hubs;

public interface IBoardActionsHubService
{
    /// <summary>
    /// Raised for every parsed <see cref="BoardActionNotification"/> received on the hub, for any
    /// board currently joined -- alongside (not instead of) this service's own forwarding to
    /// <c>IBoardDetailsStore</c>. Lets other board-scoped consumers (e.g. the call page's linked
    /// tasks) react to the same stream without a second hub connection or group. Subscribers must
    /// filter by <see cref="BoardActionNotification.BoardId"/> themselves, since a circuit may hold
    /// membership in more than one board's group at a time.
    /// </summary>
    event Action<BoardActionNotification>? BoardActionReceived;

    /// <summary>
    /// Establishes the underlying hub connection alone, without joining any board group (AD-10).
    /// Call this as soon as the user is authenticated -- e.g. from the app shell -- so a
    /// cross-page "call started" alert can reach them even if they never open a board.
    /// Idempotent: also called internally by <see cref="JoinBoardAsync"/>.
    /// </summary>
    Task ConnectAsync(CancellationToken ct = default);

    Task JoinBoardAsync(Guid boardId, CancellationToken ct = default);

    Task LeaveBoardAsync(Guid boardId, CancellationToken ct = default);

    Task DisconnectAsync(CancellationToken ct = default);
}
