using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Entities;
using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Application.Boards.Positioning;

/// <summary>
/// Applies board item position changes in two database phases to satisfy unique position indexes.
/// </summary>
/// <remarks>
/// Each batch must include every active entity in the unique-index scope
/// (all board columns or all tasks in a column). Does not begin or commit transactions.
/// </remarks>
internal sealed class BoardPositioningService(IUnitOfWork unitOfWork) : IBoardPositioningService
{
    /// <summary>
    /// Reorders an entity within a single scope and persists the new order in two database phases.
    /// </summary>
    /// <param name="items">All tracked entities in the unique-index scope (every column on a board, or every task in one column).</param>
    /// <param name="movedId">Id of the entity to move.</param>
    /// <param name="newIndex">Zero-based target index in the current order.</param>
    public async Task MoveToIndexAsync<TEntity>(
        IReadOnlyList<TEntity> items,
        Guid movedId,
        int newIndex,
        CancellationToken ct = default)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        var orderedIds = ComputeOrderAfterMoveToIndex(items, movedId, newIndex);

        if (orderedIds is null)
            return;

        await PersistOrderTwoPhaseAsync(items, orderedIds, ct);
    }

    /// <summary>
    /// Renumbers entities to contiguous zero-based positions while preserving their current sort order by <see cref="IPositionedEntity.Position"/>.
    /// </summary>
    /// <param name="items">All tracked entities in the unique-index scope (for example remaining board columns after a deletion).</param>
    public async Task ApplyCurrentOrderAsync<TEntity>(
        IReadOnlyList<TEntity> items,
        CancellationToken ct = default)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        ArgumentNullException.ThrowIfNull(items);

        var orderedIds = GetOrderedIds(items);

        await PersistOrderTwoPhaseAsync(items, orderedIds, ct);
    }

    /// <summary>
    /// Moves a task into another column at <paramref name="newIndex"/> with two-phase persistence per column scope.
    /// </summary>
    /// <param name="sourceTasks">All tracked tasks in the source column, including <paramref name="movedTask"/>.</param>
    /// <param name="targetTasks">All tracked tasks in the target column; must not include <paramref name="movedTask"/>.</param>
    /// <param name="movedTask">The task being moved.</param>
    /// <param name="targetColumnId">Destination column id; must differ from <paramref name="movedTask"/>'s current column.</param>
    /// <param name="newIndex">Zero-based insert index in the target column order.</param>
    public async Task MoveTaskToColumnAsync(
        IReadOnlyList<BoardTask> sourceTasks,
        IReadOnlyList<BoardTask> targetTasks,
        BoardTask movedTask,
        Guid targetColumnId,
        int newIndex,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(movedTask);
        ArgumentNullException.ThrowIfNull(sourceTasks);
        ArgumentNullException.ThrowIfNull(targetTasks);
        ArgumentOutOfRangeException.ThrowIfNegative(newIndex);

        if (movedTask.ColumnId == targetColumnId)
            throw new ArgumentException("Use MoveToIndexAsync to reorder a task within the same column.", nameof(targetColumnId));

        if (!sourceTasks.Any(task => task.Id == movedTask.Id))
            throw new ArgumentException("Moved task was not found in the source collection.", nameof(movedTask));

        if (targetTasks.Any(task => task.Id == movedTask.Id))
            throw new ArgumentException("Target tasks must not contain the moved task.", nameof(targetTasks));

        if (newIndex > targetTasks.Count)
            throw new ArgumentOutOfRangeException(nameof(newIndex));

        movedTask.Position = AllocateTemporaryPosition(sourceTasks);

        var sourceItems = sourceTasks
            .Where(task => task.Id != movedTask.Id)
            .OrderBy(task => task.Position)
            .ThenBy(task => task.Id)
            .ToList();

        if (sourceItems.Count > 0)
            await PersistOrderTwoPhaseAsync(sourceItems, sourceItems.Select(task => task.Id).ToList(), ct);

        movedTask.ColumnId = targetColumnId;

        movedTask.Position = AllocateTemporaryPosition(targetTasks);

        var targetItems = targetTasks
            .OrderBy(task => task.Position)
            .ThenBy(task => task.Id)
            .ToList();

        targetItems.Insert(newIndex, movedTask);

        await PersistOrderTwoPhaseAsync(targetItems, targetItems.Select(task => task.Id).ToList(), ct);
    }

    /// <summary>
    /// Moves a contiguous block of tasks into another column at the start or end of the target order with two-phase persistence on the target scope.
    /// </summary>
    /// <param name="tasksToMove">Tracked tasks leaving the source column, in the order they should appear in the target column.</param>
    /// <param name="targetTasks">All tracked tasks already in the target column; must not include any task from <paramref name="tasksToMove"/>.</param>
    /// <param name="targetColumnId">Destination column id; must differ from the current column of every task in <paramref name="tasksToMove"/>.</param>
    /// <param name="placement">Whether the block is inserted before existing target tasks or appended after them.</param>
    public async Task MoveTaskRangeToColumnAsync(
        IReadOnlyList<BoardTask> tasksToMove,
        IReadOnlyList<BoardTask> targetTasks,
        Guid targetColumnId,
        ColumnTaskMovePlacement placement,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tasksToMove);
        ArgumentNullException.ThrowIfNull(targetTasks);

        if (tasksToMove.Count == 0)
            return;

        if (!Enum.IsDefined(placement))
            throw new ArgumentOutOfRangeException(nameof(placement));

        var tasksToMoveIds = tasksToMove.Select(task => task.Id).ToHashSet();

        if (tasksToMoveIds.Count != tasksToMove.Count)
            throw new ArgumentException("Tasks to move must be unique.", nameof(tasksToMove));

        if (targetTasks.Any(task => tasksToMoveIds.Contains(task.Id)))
            throw new ArgumentException("Target tasks must not contain any task being moved.", nameof(targetTasks));

        if (tasksToMove.Any(task => task.ColumnId == targetColumnId))
            throw new ArgumentException("Tasks to move must not already belong to the target column.", nameof(targetColumnId));

        foreach (var task in tasksToMove)
            task.ColumnId = targetColumnId;

        var insertIndex = ResolveInsertIndex(placement, targetTasks.Count);

        var targetItems = targetTasks
            .OrderBy(task => task.Position)
            .ThenBy(task => task.Id)
            .ToList();

        targetItems.InsertRange(insertIndex, tasksToMove);

        await PersistOrderTwoPhaseAsync(targetItems, targetItems.Select(task => task.Id).ToList(), ct);
    }

    /// <summary>
    /// Persists tracked entity changes. Central entry point for future retry on unique-index conflicts.
    /// </summary>
    private Task SaveChangesAsync(CancellationToken ct) =>
        unitOfWork.SaveChangesAsync(ct);

    /// <summary>
    /// Validates the ordered id list, assigns temporary then final positions, and flushes twice to the database.
    /// </summary>
    private async Task PersistOrderTwoPhaseAsync<TEntity>(
        IReadOnlyList<TEntity> items,
        IReadOnlyList<Guid> orderedIds,
        CancellationToken ct)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        ValidateOrderedIds(items, orderedIds);

        if (orderedIds.Count == 0)
            return;

        var finalPositions = ToPositionsById(orderedIds);

        if (IsAlreadyApplied(items, finalPositions))
            return;

        AssignTemporaryPositions(items);
        await SaveChangesAsync(ct);

        AssignFinalPositions(items, finalPositions);
        await SaveChangesAsync(ct);
    }

    /// <summary>
    /// Builds the target order after moving <paramref name="movedId"/> to <paramref name="newIndex"/> without persisting.
    /// </summary>
    /// <returns>Ordered ids when the position changes; <see langword="null"/> when already at the target index.</returns>
    private static IReadOnlyList<Guid>? ComputeOrderAfterMoveToIndex<TEntity>(
        IReadOnlyList<TEntity> items,
        Guid movedId,
        int newIndex)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(newIndex);

        if (items.Count == 0)
            throw new ArgumentException("Items must not be empty.", nameof(items));

        if (newIndex >= items.Count)
            throw new ArgumentOutOfRangeException(nameof(newIndex));

        var orderedIds = GetOrderedIds(items);
        var currentIndex = orderedIds.IndexOf(movedId);

        if (currentIndex < 0)
            throw new ArgumentException("Moved item was not found in the collection.", nameof(movedId));

        if (currentIndex == newIndex)
            return null;

        orderedIds.RemoveAt(currentIndex);
        orderedIds.Insert(newIndex, movedId);

        return orderedIds;
    }

    /// <summary>
    /// Maps an ordered id list to zero-based final positions (index in the list becomes position value).
    /// </summary>
    private static IReadOnlyDictionary<Guid, int> ToPositionsById(IReadOnlyList<Guid> orderedIds) =>
        orderedIds
            .Select((id, index) => (id, index))
            .ToDictionary(x => x.id, x => x.index);

    /// <summary>
    /// Phase 1: assigns unique temporary positions above the current max to avoid unique-index collisions on save.
    /// </summary>
    private static void AssignTemporaryPositions<TEntity>(IReadOnlyList<TEntity> items)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        var count = items.Count;
        var maxPosition = items.Max(item => item.Position);

        for (var i = 0; i < count; i++)
            items[i].Position = maxPosition + 1 + i;
    }

    /// <summary>
    /// Phase 2: assigns final zero-based positions from the precomputed map.
    /// </summary>
    private static void AssignFinalPositions<TEntity>(
        IReadOnlyList<TEntity> items,
        IReadOnlyDictionary<Guid, int> finalPositions)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        foreach (var item in items)
            item.Position = finalPositions[item.Id];
    }

    /// <summary>
    /// Returns a temporary position that does not collide with any task currently in the column scope.
    /// </summary>
    private static int AllocateTemporaryPosition(IReadOnlyList<BoardTask> scopeTasks) =>
        scopeTasks.Count == 0
            ? 0
            : scopeTasks.Max(task => task.Position) + scopeTasks.Count + 1;

    /// <summary>
    /// Maps <paramref name="placement"/> to a zero-based insert index in the current target column order.
    /// </summary>
    private static int ResolveInsertIndex(ColumnTaskMovePlacement placement, int targetTaskCount) =>
        placement switch
        {
            ColumnTaskMovePlacement.Start => 0,
            ColumnTaskMovePlacement.End => targetTaskCount,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };

    /// <summary>
    /// Returns ids sorted by current <see cref="IPositionedEntity.Position"/>, using id as a stable tie-breaker.
    /// </summary>
    private static List<Guid> GetOrderedIds<TEntity>(IReadOnlyList<TEntity> items)
        where TEntity : Entity<Guid>, IPositionedEntity =>
            items
                .OrderBy(item => item.Position)
                .ThenBy(item => item.Id)
                .Select(item => item.Id)
                .ToList();

    /// <summary>
    /// Returns whether every entity already has the target position (no database writes needed).
    /// </summary>
    private static bool IsAlreadyApplied<TEntity>(IReadOnlyList<TEntity> items, IReadOnlyDictionary<Guid, int> finalPositions)
        where TEntity : Entity<Guid>, IPositionedEntity =>
            items.All(item => item.Position == finalPositions[item.Id]);

    /// <summary>
    /// Ensures <paramref name="orderedIds"/> is a complete, unique permutation of <paramref name="items"/>.
    /// </summary>
    private static void ValidateOrderedIds<TEntity>(IReadOnlyList<TEntity> items, IReadOnlyList<Guid> orderedIds)
        where TEntity : Entity<Guid>, IPositionedEntity
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(orderedIds);

        if (items.Count != orderedIds.Count)
            throw new ArgumentException("Ordered ids must include every item exactly once.");

        if (orderedIds.Distinct().Count() != orderedIds.Count)
            throw new ArgumentException("Ordered ids must be unique.");

        var itemIds = items.Select(item => item.Id).ToHashSet();

        if (orderedIds.Any(id => !itemIds.Contains(id)))
            throw new ArgumentException("Ordered ids must match the provided items.");
    }
}
