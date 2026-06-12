using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Exceptions;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardDetailsStore(IBoardApiService boardApiService) : IBoardDetailsStore
{
    public Guid? BoardId { get; private set; }
    public BoardDetailsDto? Board { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task LoadAsync(Guid boardId, CancellationToken ct = default)
    {
        if (BoardId == boardId && Board is not null && !IsLoading)
            return;

        BoardId = boardId;
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            Board = await boardApiService.GetBoardByIdAsync(boardId, ct);
        }
        catch (ApiServiceException ex)
        {
            Board = null;
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details
                ? string.Join(" ", details)
                : ex.Message;
        }
        catch (Exception ex)
        {
            Board = null;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public async Task<ColumnDto> CreateColumnAsync(string name, CancellationToken ct = default)
    {
        if (BoardId is not { } boardId || Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        var column = await boardApiService.CreateColumnAsync(boardId, name, ct);
        AddColumn(column);

        return column;
    }

    public async Task<TaskLookupDto> CreateTaskAsync(Guid columnId, string title, CancellationToken ct = default)
    {
        if (BoardId is not { } boardId || Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        var task = await boardApiService.CreateTaskAsync(boardId, columnId, title, ct);
        AddTask(columnId, task);

        return task;
    }

    public async Task DeleteColumnAsync(Guid columnId, DeleteColumnRequest request, CancellationToken ct = default)
    {
        if (BoardId is not { } boardId || Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        var column = Board.Columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column was not found in the loaded board.");

        await boardApiService.DeleteColumnAsync(boardId, columnId, request, ct);
        ApplyColumnDeletion(column, request);
    }

    public void UpdateBoardName(string name)
    {
        if (Board is null)
            return;

        Board = Board with { Name = name };
        NotifyStateChanged();
    }

    public void UpdateColumnName(Guid columnId, string name)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Select(column => column.Id == columnId ? column with { Name = name } : column)
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    public async Task ReorderColumnAsync(Guid columnId, int position, CancellationToken ct = default)
    {
        if (BoardId is not { } boardId || Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        var orderedColumns = Board.Columns
            .OrderBy(column => column.Position)
            .ToList();

        var currentIndex = orderedColumns.FindIndex(column => column.Id == columnId);

        if (currentIndex < 0)
            throw new InvalidOperationException("Column was not found on the board.");

        if (currentIndex == position)
            return;

        var previousOrder = orderedColumns
            .Select(column => column.Id)
            .ToList();

        ApplyColumnMove(columnId, position);

        try
        {
            await boardApiService.ReorderColumnAsync(boardId, columnId, position, ct);
        }
        catch
        {
            ApplyColumnOrder(previousOrder);
            throw;
        }
    }

    public void Reset()
    {
        BoardId = null;
        Board = null;
        IsLoading = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    private void ApplyColumnDeletion(ColumnDto deletedColumn, DeleteColumnRequest request)
    {
        if (Board is null)
            return;

        var columns = Board.Columns.ToList();
        var deletedPosition = deletedColumn.Position;

        if (request.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn
            && deletedColumn.BoardTasks.Count > 0
            && request.TargetColumnId is { } targetColumnId)
        {
            columns = MoveTasksLocally(
                columns,
                deletedColumn.BoardTasks,
                targetColumnId,
                request.MovePlacement!.Value);
        }

        columns = columns
            .Where(column => column.Id != deletedColumn.Id)
            .Select(column => column.Position > deletedPosition
                ? column with { Position = column.Position - 1 }
                : column)
            .OrderBy(column => column.Position)
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    private static List<ColumnDto> MoveTasksLocally(
        List<ColumnDto> columns,
        IReadOnlyList<TaskLookupDto> tasksToMove,
        Guid targetColumnId,
        ColumnTaskMovePlacement placement)
    {
        var targetColumn = columns.First(column => column.Id == targetColumnId);
        var targetTasks = targetColumn.BoardTasks.ToList();
        IReadOnlyList<TaskLookupDto> updatedTargetTasks;

        if (placement == ColumnTaskMovePlacement.Start)
        {
            var offset = tasksToMove.Count;
            var shiftedTargetTasks = targetTasks
                .Select(task => task with { Position = task.Position + offset })
                .ToList();
            var movedTasks = tasksToMove
                .Select((task, index) => task with { Position = index })
                .ToList();

            updatedTargetTasks = movedTasks
                .Concat(shiftedTargetTasks)
                .OrderBy(task => task.Position)
                .ToList();
        }
        else
        {
            var startPosition = targetTasks.Count;
            var movedTasks = tasksToMove
                .Select((task, index) => task with { Position = startPosition + index })
                .ToList();

            updatedTargetTasks = targetTasks
                .Concat(movedTasks)
                .ToList();
        }

        return columns
            .Select(column => column.Id == targetColumnId
                ? column with { BoardTasks = updatedTargetTasks }
                : column)
            .ToList();
    }

    private void AddColumn(ColumnDto column)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Append(column)
            .OrderBy(c => c.Position)
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    private void AddTask(Guid columnId, TaskLookupDto task)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Select(column => column.Id == columnId
                ? column with
                {
                    BoardTasks = column.BoardTasks
                        .Append(task)
                        .OrderBy(t => t.Position)
                        .ToList()
                }
                : column)
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    private void ApplyColumnMove(Guid columnId, int newIndex)
    {
        if (Board is null)
            return;

        var orderedColumns = Board.Columns
            .OrderBy(column => column.Position)
            .ToList();

        var currentIndex = orderedColumns.FindIndex(boardColumn => boardColumn.Id == columnId);
        var column = orderedColumns[currentIndex];
        orderedColumns.RemoveAt(currentIndex);
        orderedColumns.Insert(newIndex, column);

        ApplyColumnOrder(orderedColumns.Select(boardColumn => boardColumn.Id).ToList());
    }

    private void ApplyColumnOrder(IReadOnlyList<Guid> columnIds)
    {
        if (Board is null)
            return;

        var columnsById = Board.Columns.ToDictionary(column => column.Id);

        var reorderedColumns = columnIds
            .Select((id, index) => columnsById[id] with { Position = index })
            .ToList();

        Board = Board with { Columns = reorderedColumns };
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
