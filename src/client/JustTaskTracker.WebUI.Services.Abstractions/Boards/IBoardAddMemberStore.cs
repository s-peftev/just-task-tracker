using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for user lookup inside the add-member tab of the board members overlay.
/// </summary>
public interface IBoardAddMemberStore
{
    string SearchText { get; }
    IReadOnlyList<UserForBoardLookupDto> Users { get; }
    PaginationMetadata Pagination { get; }
    int CurrentPage { get; }
    bool IsAttached { get; }
    bool IsLoading { get; }
    bool HasMoreUsers { get; }
    bool IsLoadingMoreUsers { get; }

    event Action? StateChanged;

    void Attach(Guid boardId);

    Task SetSearchAsync(string searchText, CancellationToken ct = default);

    Task LoadMoreAsync(CancellationToken ct = default);

    void Reset();
}
