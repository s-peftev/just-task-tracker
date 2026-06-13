using JustTaskTracker.WebUI.Domain.Boards;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for the board task details modal (single task at a time).
/// </summary>
public interface IBoardTaskStore
{
    Guid? BoardId { get; }
    Guid? ColumnId { get; }
    Guid? TaskId { get; }
    BoardTaskDetailsDto? Task { get; }
    bool IsLoading { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    Task LoadAsync(Guid boardId, Guid columnId, Guid taskId, CancellationToken ct = default);

    void UpdateTaskTitle(string title);

    void Reset();
}
