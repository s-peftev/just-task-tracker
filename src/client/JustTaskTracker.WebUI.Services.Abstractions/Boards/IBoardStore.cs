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

    event Action? StateChanged;

    Task LoadActiveAsync(int pageNumber, CancellationToken ct = default);

    Task LoadArchivedAsync(int pageNumber, CancellationToken ct = default);

    Task SetActiveSearchAsync(string searchText, CancellationToken ct = default);

    Task SetArchivedSearchAsync(string searchText, CancellationToken ct = default);

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
