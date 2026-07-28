namespace JustTaskTracker.WebUI.Services.Abstractions.Hubs;

public interface IBoardActionsHubService
{
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
