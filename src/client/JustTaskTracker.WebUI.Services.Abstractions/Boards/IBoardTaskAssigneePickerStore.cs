using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for the board-member picker opened from the task assignee control.
/// </summary>
public interface IBoardTaskAssigneePickerStore
{
    Guid? BoardId { get; }
    IReadOnlyList<BoardMemberDto> Members { get; }
    PaginationMetadata Pagination { get; }
    bool IsOpen { get; }
    bool IsLoading { get; }
    bool HasMoreMembers { get; }
    bool IsLoadingMoreMembers { get; }
    string SearchText { get; }

    event Action? StateChanged;

    Task OpenAsync(Guid boardId, CancellationToken ct = default);

    Task SetSearchAsync(string searchText, CancellationToken ct = default);

    Task LoadMoreAsync(CancellationToken ct = default);

    void Close();
}
