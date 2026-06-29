using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Requests;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for a single board details page (columns, tasks, permissions).
/// </summary>
public interface IBoardDetailsStore
{
    Guid? BoardId { get; }
    BoardDetailsDto? Board { get; }
    bool IsLoading { get; }
    string? ErrorMessage { get; }
    bool IsReorderingTasks { get; }
    bool ShowOnlyMyTasks { get; }
    bool IsReadOnly { get; }

    event Action? StateChanged;

    Task LoadAsync(Guid boardId, CancellationToken ct = default);

    Task<ColumnDto> CreateColumnAsync(string name, CancellationToken ct = default);

    Task<BoardTaskPreviewDto> CreateTaskAsync(Guid columnId, string title, CancellationToken ct = default);

    void UpdateBoardName(string name);

    void SetBoardArchived(
        DateTime archivedAtUtc,
        BoardSerializationStatus boardSerializationStatus,
        BoardArchiveExportOptions? exportOptions = null);

    void UpdateColumnName(Guid columnId, string name);

    void UpdateTaskTitle(Guid taskId, string title);

    void AdjustTaskCommentsCount(Guid taskId, int delta);

    void AdjustTaskAttachmentsCount(Guid taskId, int delta);

    void UpdateTaskAssigneeId(Guid taskId, Guid? assigneeId);

    void SetShowOnlyMyTasks(bool showOnlyMyTasks);

    Task DeleteColumnAsync(Guid columnId, DeleteColumnRequest request, CancellationToken ct = default);

    Task ReorderColumnAsync(Guid columnId, int position, CancellationToken ct = default);

    Task ReorderTaskAsync(Guid taskId, Guid targetColumnId, int position, CancellationToken ct = default);

    Task DeleteTaskAsync(Guid boardId, Guid columnId, Guid taskId, CancellationToken ct = default);

    void Reset();
}
