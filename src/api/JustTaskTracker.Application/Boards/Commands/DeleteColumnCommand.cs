using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record DeleteColumnCommand(
    Guid BoardId,
    Guid ColumnId,
    DeleteColumnTasksDisposition TasksDisposition,
    Guid? TargetColumnId = null,
    ColumnTaskMovePlacement? MovePlacement = null) : IRequest<Result>;

public class DeleteColumnCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteColumnCommand, Result>
{
    public async Task<Result> Handle(DeleteColumnCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (userRole is not { } role || !BoardRolePermissions.CanManageColumns(role))
            return Result.Failure(GeneralErrors.Forbidden);

        var column = await columnRepository.GetByBoardIdAndIdAsync(
            request.BoardId,
            request.ColumnId,
            ct);

        if (column is null)
            return Result.Failure(GeneralErrors.NotFound);

        var columnTasks = await boardTaskRepository.GetOrderedByColumnIdAsync(request.ColumnId, ct);

        if (request.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn)
        {
            var targetColumn = await columnRepository.GetByBoardIdAndIdAsync(
                request.BoardId,
                request.TargetColumnId!.Value,
                ct);

            if (targetColumn is null)
                return Result.Failure(GeneralErrors.NotFound);

            if (request.MovePlacement == ColumnTaskMovePlacement.Start)
            {
                var targetTasks = await boardTaskRepository.GetOrderedByColumnIdAsync(targetColumn.Id, ct);
                MoveTasksToStart(columnTasks, targetTasks, targetColumn.Id);
            }
            else if (request.MovePlacement == ColumnTaskMovePlacement.End)
            {
                var targetTaskCount = await boardTaskRepository.GetCountByColumnIdAsync(targetColumn.Id, ct);
                MoveTasksToEnd(columnTasks, targetTaskCount, targetColumn.Id);
            }
        }
        else if (columnTasks.Count > 0)
        {
            boardTaskRepository.RemoveRange(columnTasks);
        }

        columnRepository.Remove(column);

        var columnsToReposition = await columnRepository.GetWithPositionGreaterThanAsync(
            request.BoardId,
            column.Position,
            ct);

        foreach (var columnToReposition in columnsToReposition)
            columnToReposition.Position--;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static void MoveTasksToStart(
        IReadOnlyList<BoardTask> tasksToMove,
        IReadOnlyList<BoardTask> targetTasks,
        Guid targetColumnId)
    {
        var offset = tasksToMove.Count;

        foreach (var targetTask in targetTasks)
            targetTask.Position += offset;

        for (var i = 0; i < tasksToMove.Count; i++)
        {
            tasksToMove[i].ColumnId = targetColumnId;
            tasksToMove[i].Position = i;
        }
    }

    private static void MoveTasksToEnd(
        IReadOnlyList<BoardTask> tasksToMove,
        int targetTaskCount,
        Guid targetColumnId)
    {
        for (var i = 0; i < tasksToMove.Count; i++)
        {
            tasksToMove[i].ColumnId = targetColumnId;
            tasksToMove[i].Position = targetTaskCount + i;
        }
    }
}
