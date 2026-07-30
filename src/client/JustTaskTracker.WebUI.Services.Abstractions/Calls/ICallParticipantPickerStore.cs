using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

/// <summary>
/// Scoped store for the "Restricted call" participant picker opened from the create-call form.
/// Board-member listing/search/pagination works like the task assignee picker's store, but this
/// store additionally owns the working allow-list selection itself (<see cref="SelectedUserIds"/>)
/// so it survives the picker overlay being closed and reopened within the same create-call dialog session --
/// only <see cref="Reset"/> (called once per fresh dialog) clears it back to just the creator.
/// </summary>
public interface ICallParticipantPickerStore
{
    Guid? BoardId { get; }
    Guid? CurrentUserId { get; }
    IReadOnlyList<BoardMemberDto> Members { get; }
    PaginationMetadata Pagination { get; }
    bool IsOpen { get; }
    bool IsLoading { get; }
    bool HasMoreMembers { get; }
    bool IsLoadingMoreMembers { get; }
    string SearchText { get; }

    /// <summary>The current allow-list selection, creator first, in the order members were added.</summary>
    IReadOnlyList<Guid> SelectedUserIds { get; }

    /// <summary>Selected members other than the creator, resolved from a cache that survives search filtering.</summary>
    IReadOnlyList<BoardMemberDto> AddedMembers { get; }

    event Action? StateChanged;

    /// <summary>Seeds the selection with just <paramref name="currentUserId"/>. Call once per fresh create-call dialog.</summary>
    void Reset(Guid currentUserId);

    Task OpenAsync(Guid boardId, CancellationToken ct = default);

    Task SetSearchAsync(string searchText, CancellationToken ct = default);

    Task LoadMoreAsync(CancellationToken ct = default);

    void Close();

    bool IsSelected(Guid userId);

    /// <summary>Adds or removes <paramref name="member"/> from the selection. No-op for the creator's own id.</summary>
    void Toggle(BoardMemberDto member);
}
