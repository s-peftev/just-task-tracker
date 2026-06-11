using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for the current user's boards list shown on the dashboard.
/// Tracks pagination state and caches each board's role so that board-scoped
/// pages can resolve the current user's permissions without an extra request.
/// </summary>
public interface IBoardStore
{
    IReadOnlyList<BoardLookupDto> Boards { get; }
    PaginationMetadata Pagination { get; }
    int CurrentPage { get; }
    string SearchText { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    /// <summary>Loads the given page. No-ops if the same page is already loaded and no refresh is forced.</summary>
    Task LoadAsync(int pageNumber, CancellationToken ct = default);

    /// <summary>Updates the name search filter and reloads page 1 after a short debounce.</summary>
    Task SetSearchAsync(string searchText, CancellationToken ct = default);

    /// <summary>Reloads the current page from the API.</summary>
    Task RefreshAsync(CancellationToken ct = default);

    /// <summary>Returns the current user's role on a board if it has been seen in any loaded page; otherwise null.</summary>
    BoardMemberRole? GetCachedRole(Guid boardId);

    /// <summary>Clears all cached state. Call on logout.</summary>
    void Reset();
}
