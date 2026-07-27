using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

/// <summary>
/// Scoped store for the "Link board tasks" picker opened from the create-call form (AD-13).
/// Task listing/search/pagination works like the board's own task-search overlay, but this
/// store additionally owns the working link selection itself (<see cref="SelectedTaskIds"/>)
/// so it survives the picker overlay being closed and reopened within the same create-call dialog session --
/// only <see cref="Reset"/> (called once per fresh dialog) clears it.
/// </summary>
public interface ICallTaskLinkPickerStore
{
    Guid? BoardId { get; }
    IReadOnlyList<BoardTaskLookupDto> Tasks { get; }
    PaginationMetadata Pagination { get; }
    bool IsOpen { get; }
    bool IsLoading { get; }
    bool HasMoreTasks { get; }
    bool IsLoadingMoreTasks { get; }
    string SearchText { get; }

    /// <summary>The current link selection, in the order tasks were added.</summary>
    IReadOnlyList<Guid> SelectedTaskIds { get; }

    /// <summary>Selected tasks, resolved from a cache that survives search filtering.</summary>
    IReadOnlyList<BoardTaskLookupDto> LinkedTasks { get; }

    event Action? StateChanged;

    /// <summary>Clears the selection. Call once per fresh create-call dialog.</summary>
    void Reset();

    Task OpenAsync(Guid boardId, Guid columnId, CancellationToken ct = default);

    Task SetSearchAsync(string searchText, CancellationToken ct = default);

    Task LoadMoreAsync(CancellationToken ct = default);

    void Close();

    bool IsSelected(Guid taskId);

    /// <summary>Adds or removes <paramref name="task"/> from the selection.</summary>
    void Toggle(BoardTaskLookupDto task);
}
