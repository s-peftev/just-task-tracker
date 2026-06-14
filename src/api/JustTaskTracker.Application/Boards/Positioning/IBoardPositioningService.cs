using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Entities;
using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Application.Boards.Positioning;

/// <summary>
/// Persists board column and task position changes using two-phase updates for unique position indexes.
/// </summary>
public interface IBoardPositioningService
{
    /// <summary>
    /// Reorders an entity within a single scope (board columns or tasks in one column).
    /// </summary>
    Task MoveToIndexAsync<TEntity>(
        IReadOnlyList<TEntity> items,
        Guid movedId,
        int newIndex,
        CancellationToken ct = default)
        where TEntity : Entity<Guid>, IPositionedEntity;

    /// <summary>
    /// Renumbers entities to contiguous zero-based positions while preserving their current order by position.
    /// </summary>
    Task ApplyCurrentOrderAsync<TEntity>(
        IReadOnlyList<TEntity> items,
        CancellationToken ct = default)
        where TEntity : Entity<Guid>, IPositionedEntity;

    /// <summary>
    /// Moves a task into another column at <paramref name="newIndex"/>. Use <see cref="MoveToIndexAsync"/> for reordering within the same column.
    /// </summary>
    Task MoveTaskToColumnAsync(
        IReadOnlyList<BoardTask> sourceTasks,
        IReadOnlyList<BoardTask> targetTasks,
        BoardTask movedTask,
        Guid targetColumnId,
        int newIndex,
        CancellationToken ct = default);

    /// <summary>
    /// Moves a contiguous block of tasks into another column at the start or end of the target order. Does not compact the source column.
    /// </summary>
    Task MoveTaskRangeToColumnAsync(
        IReadOnlyList<BoardTask> tasksToMove,
        IReadOnlyList<BoardTask> targetTasks,
        Guid targetColumnId,
        ColumnTaskMovePlacement placement,
        CancellationToken ct = default);
}
