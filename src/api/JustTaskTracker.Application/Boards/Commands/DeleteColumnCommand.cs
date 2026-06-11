using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
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
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanManageColumns) is { } failure)
            return failure;

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

        var columnsToReposition = await columnRepository.GetListWithPositionGreaterThanAsync(
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

public class DeleteColumnCommandValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.TasksDisposition)
            .IsInEnum();

        When(x => x.TasksDisposition == DeleteColumnTasksDisposition.DeleteWithColumn, () =>
        {
            RuleFor(x => x.TargetColumnId)
                .Null()
                .WithMessage("'Target Column Id' must be null when deleting tasks with the column.");

            RuleFor(x => x.MovePlacement)
                .Null()
                .WithMessage("'Move Placement' must be null when deleting tasks with the column.");
        });

        When(x => x.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn, () =>
        {
            RuleFor(x => x.TargetColumnId)
                .NotEmpty()
                .WithMessage("'Target Column Id' is required when moving tasks to another column.");

            RuleFor(x => x.MovePlacement)
                .NotNull()
                .IsInEnum()
                .WithMessage("'Move Placement' is required when moving tasks to another column.");

            RuleFor(x => x)
                .Must(x => x.TargetColumnId != x.ColumnId)
                .WithMessage("Tasks cannot be moved to the column being deleted.");
        });
    }
}
