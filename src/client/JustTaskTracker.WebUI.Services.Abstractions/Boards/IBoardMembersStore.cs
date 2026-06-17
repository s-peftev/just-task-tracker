using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

public enum BoardMembersOverlayTab
{
    Members,
    AddMember,
}

/// <summary>
/// Scoped store for the board members overlay on the board page.
/// </summary>
public interface IBoardMembersStore
{
    Guid? BoardId { get; }
    BoardMemberRole UserRole { get; }
    BoardMembersOverlayTab ActiveTab { get; }
    IReadOnlyList<BoardMemberDto> Members { get; }
    PaginationMetadata Pagination { get; }
    int CurrentPage { get; }
    bool IsOpen { get; }
    bool IsLoading { get; }
    bool HasMoreMembers { get; }
    bool IsLoadingMoreMembers { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    Task OpenAsync(Guid boardId, BoardMemberRole userRole, CancellationToken ct = default);

    void SetActiveTab(BoardMembersOverlayTab tab);

    Task LoadMoreAsync(CancellationToken ct = default);

    void Close();

    void Reset();
}
