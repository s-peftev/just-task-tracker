using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for the current user's boards list shown on the boards page.
/// Tracks pagination state per active/archived section and caches each board's role
/// so that board-scoped pages can resolve permissions without an extra request.
/// </summary>
public interface IBoardStore
{
    BoardListSectionState Active { get; }
    BoardListSectionState Archived { get; }

    /// <summary>
    /// Count of non-archived boards where the current user is Owner.
    /// Null until the first successful active-section load.
    /// </summary>
    int? ActiveOwnedBoardsCount { get; }

    /// <summary>
    /// When <c>true</c>, list requests include <c>IsOwned=true</c>.
    /// When <c>null</c>, ownership is not filtered. <c>false</c> is unused by the UI.
    /// </summary>
    bool? IsOwnedFilter { get; }

    event Action? StateChanged;

    Task LoadActiveAsync(int pageNumber, CancellationToken ct = default);

    Task LoadArchivedAsync(int pageNumber, CancellationToken ct = default);

    Task SetActiveSearchAsync(string searchText, CancellationToken ct = default);

    Task SetArchivedSearchAsync(string searchText, CancellationToken ct = default);

    /// <summary>
    /// Updates the owned filter without issuing a network request.
    /// Used when hydrating UI preference before the first refresh.
    /// </summary>
    void SyncIsOwnedFilter(bool? isOwned);

    /// <summary>
    /// Sets the owned-only filter and reloads list(s) from page 1.
    /// </summary>
    Task SetIsOwnedFilterAsync(bool? isOwned, bool includeArchived, CancellationToken ct = default);

    Task RefreshAsync(bool includeArchived = true, CancellationToken ct = default);

    BoardMemberRole? GetCachedRole(Guid boardId);

    void ApplyBoardArchived(Guid boardId, DateTime archivedAtUtc, BoardExportStatus boardExportStatus);

    void ApplyBoardReExportPending(Guid boardId);

    void ApplyExportStatusChanged(Guid boardId, BoardExportStatus status);

    void ApplyReExportStatusChanged(Guid boardId, BoardExportStatus status);

    void Reset();

    /// <summary>
    /// Clears archived-section list state (boards, search, pagination) without
    /// touching the active section or role cache.
    /// </summary>
    void ResetArchived();
}
