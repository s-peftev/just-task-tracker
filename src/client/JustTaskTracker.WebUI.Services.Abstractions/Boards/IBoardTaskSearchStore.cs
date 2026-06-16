using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for the board-page task search overlay.
/// </summary>
public interface IBoardTaskSearchStore
{
    IReadOnlyList<BoardTaskLookupDto> Tasks { get; }
    Guid? BoardId { get; }
    PaginationMetadata Pagination { get; }
    int CurrentPage { get; }
    string SearchText { get; }
    bool IsOpen { get; }
    bool IsLoading { get; }
    bool HasMoreTasks { get; }
    bool IsLoadingMoreTasks { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    Task OpenAsync(Guid boardId, Guid columnId, CancellationToken ct = default);

    Task SetSearchAsync(string searchText, CancellationToken ct = default);

    Task LoadMoreAsync(CancellationToken ct = default);

    void Close();

    void Reset();
}
