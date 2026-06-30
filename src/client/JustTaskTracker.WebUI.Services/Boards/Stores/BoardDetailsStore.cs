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
    public bool IsReorderingTasks { get; private set; }
    public bool ShowOnlyMyTasks { get; private set; }
    public bool IsReadOnly => Board?.IsArchived == true;

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

    public async Task<BoardTaskPreviewDto> CreateTaskAsync(Guid columnId, string title, CancellationToken ct = default)
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

    public void SetBoardArchived(
        DateTime archivedAtUtc,
        BoardExportStatus boardExportStatus,
        BoardExportOptions? exportOptions = null)
    {
        if (Board is null)
            return;

        Board = Board with
        {
            IsArchived = true,
            ArchivedAtUtc = archivedAtUtc,
            BoardExportStatus = boardExportStatus,
            ExportOptions = exportOptions,
        };

        NotifyStateChanged();
    }

    public void SetBoardReExportPending(BoardExportOptions reExportOptions)
    {
        if (Board is null)
            return;

        Board = Board with
        {
            ReExportStatus = BoardExportStatus.Pending,
            ReExportOptions = reExportOptions,
        };

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

    public void UpdateTaskTitle(Guid taskId, string title)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Select(column => column with
            {
                BoardTasks = column.BoardTasks
                    .Select(task => task.Id == taskId ? task with { Title = title } : task)
                    .ToList()
            })
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    public void AdjustTaskCommentsCount(Guid taskId, int delta)
    {
        if (Board is null || delta == 0)
            return;

        UpdateTaskPreview(taskId, task => task with
        {
            CommentsCount = Math.Max(0, task.CommentsCount + delta),
        });
    }

    public void AdjustTaskAttachmentsCount(Guid taskId, int delta)
    {
        if (Board is null || delta == 0)
            return;

        UpdateTaskPreview(taskId, task => task with
        {
            AttachmentsCount = Math.Max(0, task.AttachmentsCount + delta),
        });
    }

    public void UpdateTaskAssigneeId(Guid taskId, Guid? assigneeId)
    {
        if (Board is null)
            return;

        UpdateTaskPreview(taskId, task => task with { AssigneeId = assigneeId });
    }

    public void SetShowOnlyMyTasks(bool showOnlyMyTasks)
    {
        if (ShowOnlyMyTasks == showOnlyMyTasks)
            return;

        ShowOnlyMyTasks = showOnlyMyTasks;
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

    public async Task DeleteTaskAsync(Guid boardId, Guid columnId, Guid taskId, CancellationToken ct = default)
    {
        await boardApiService.DeleteBoardTaskAsync(boardId, columnId, taskId, ct);

        if (BoardId == boardId && Board is not null)
            RemoveTaskLocally(columnId, taskId);
    }

    public async Task ReorderTaskAsync(Guid taskId, Guid targetColumnId, int position, CancellationToken ct = default)
    {
        if (BoardId is not { } boardId || Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        var sourceColumn = Board.Columns.FirstOrDefault(column => column.BoardTasks.Any(task => task.Id == taskId));

        if (sourceColumn is null)
            throw new InvalidOperationException("Task was not found on the loaded board.");

        var task = sourceColumn.BoardTasks.First(t => t.Id == taskId);

        if (sourceColumn.Id == targetColumnId && task.Position == position)
            return;

        IsReorderingTasks = true;

        var snapshot = CaptureTaskOrderSnapshot();

        ApplyTaskMove(taskId, targetColumnId, position);

        try
        {
            await boardApiService.ReorderTaskAsync(boardId, targetColumnId, taskId, position, ct);
        }
        catch
        {
            RestoreTaskOrderSnapshot(snapshot);
            throw;
        }
        finally
        {
            IsReorderingTasks = false;
            NotifyStateChanged();
        }
    }

    public void Reset()
    {
        BoardId = null;
        Board = null;
        IsLoading = false;
        ErrorMessage = null;
        ShowOnlyMyTasks = false;
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
        IReadOnlyList<BoardTaskPreviewDto> tasksToMove,
        Guid targetColumnId,
        ColumnTaskMovePlacement placement)
    {
        var targetColumn = columns.First(column => column.Id == targetColumnId);
        var targetTasks = targetColumn.BoardTasks.ToList();
        IReadOnlyList<BoardTaskPreviewDto> updatedTargetTasks;

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

    private void RemoveTaskLocally(Guid columnId, Guid taskId)
    {
        if (Board is null)
            return;

        var column = Board.Columns.FirstOrDefault(c => c.Id == columnId);

        if (column is null || column.BoardTasks.All(task => task.Id != taskId))
            return;

        var columns = Board.Columns
            .Select(boardColumn =>
            {
                if (boardColumn.Id != columnId)
                    return boardColumn;

                var tasks = boardColumn.BoardTasks
                    .Where(task => task.Id != taskId)
                    .OrderBy(task => task.Position)
                    .Select((task, index) => task with { Position = index })
                    .ToList();

                return boardColumn with { BoardTasks = tasks };
            })
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    private void AddTask(Guid columnId, BoardTaskPreviewDto task)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Select(column =>
            {
                if (column.Id != columnId)
                    return column;

                var tasks = column.BoardTasks
                    .OrderBy(t => t.Position)
                    .Append(task)
                    .Select((t, index) => t with { Position = index })
                    .ToList();

                return column with { BoardTasks = tasks };
            })
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

    private void ApplyTaskMove(Guid taskId, Guid targetColumnId, int newIndex)
    {
        if (Board is null)
            return;

        var columns = Board.Columns.ToList();
        var sourceColumn = columns.First(column => column.BoardTasks.Any(task => task.Id == taskId));
        var movedTask = sourceColumn.BoardTasks.First(task => task.Id == taskId);

        if (sourceColumn.Id == targetColumnId)
        {
            var orderedTasks = sourceColumn.BoardTasks
                .OrderBy(task => task.Position)
                .ToList();

            orderedTasks.RemoveAll(task => task.Id == taskId);
            orderedTasks.Insert(newIndex, movedTask);

            var reorderedTasks = orderedTasks
                .Select((task, index) => task with { Position = index })
                .ToList();

            columns = columns
                .Select(column => column.Id == targetColumnId
                    ? column with { BoardTasks = reorderedTasks }
                    : column)
                .ToList();
        }
        else
        {
            var sourceTasks = sourceColumn.BoardTasks
                .Where(task => task.Id != taskId)
                .OrderBy(task => task.Position)
                .Select((task, index) => task with { Position = index })
                .ToList();

            var targetColumn = columns.First(column => column.Id == targetColumnId);
            var targetTasks = targetColumn.BoardTasks
                .OrderBy(task => task.Position)
                .ToList();

            targetTasks.Insert(newIndex, movedTask);

            var reorderedTargetTasks = targetTasks
                .Select((task, index) => task with { Position = index })
                .ToList();

            columns = columns
                .Select(column =>
                {
                    if (column.Id == sourceColumn.Id)
                        return column with { BoardTasks = sourceTasks };

                    if (column.Id == targetColumnId)
                        return column with { BoardTasks = reorderedTargetTasks };

                    return column;
                })
                .ToList();
        }

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    private TaskOrderSnapshot CaptureTaskOrderSnapshot()
    {
        if (Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        return new TaskOrderSnapshot(
            Board.Columns
                .Select(column => (column.Id, (IReadOnlyList<BoardTaskPreviewDto>)column.BoardTasks.ToList()))
                .ToList());
    }

    private void RestoreTaskOrderSnapshot(TaskOrderSnapshot snapshot)
    {
        if (Board is null)
            return;

        var tasksByColumnId = snapshot.Columns.ToDictionary(entry => entry.ColumnId, entry => entry.Tasks);

        Board = Board with
        {
            Columns = Board.Columns
                .Select(column => column with { BoardTasks = tasksByColumnId[column.Id] })
                .ToList()
        };

        NotifyStateChanged();
    }

    private sealed record TaskOrderSnapshot(
        IReadOnlyList<(Guid ColumnId, IReadOnlyList<BoardTaskPreviewDto> Tasks)> Columns);

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private void UpdateTaskPreview(Guid taskId, Func<BoardTaskPreviewDto, BoardTaskPreviewDto> update)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Select(column => column with
            {
                BoardTasks = column.BoardTasks
                    .Select(task => task.Id == taskId ? update(task) : task)
                    .ToList()
            })
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }
}
