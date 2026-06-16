using FluentValidation;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Columns;

public record DeleteColumnCommand(
    Guid BoardId,
    Guid ColumnId,
    DeleteColumnTasksDisposition TasksDisposition,
    Guid? TargetColumnId = null,
    ColumnTaskMovePlacement? MovePlacement = null) 
    : IRequest<Result>;

public class DeleteColumnCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteColumnCommand, Result>
{
    public async Task<Result> Handle(DeleteColumnCommand request, CancellationToken ct)
    {
        var userRole = await columnRepository.GetUserRoleAsync(request.ColumnId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageColumns(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var boardColumns = await columnRepository.GetListByBoardIdAsync(request.BoardId, ct);

        var column = boardColumns
            .FirstOrDefault(boardColumn => boardColumn.Id == request.ColumnId);

        if (column is null)
            return Result.Failure(GeneralErrors.NotFound);
        
        var columnBoardTasks = await boardTaskRepository.GetListByColumnIdAsync(request.ColumnId, ct);

        IReadOnlyList<BoardTask> targetBoardTasks = [];

        if (request.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn)
        {
            var targetColumn = await columnRepository.GetByIdAsync(request.TargetColumnId!.Value, ct);

            if (targetColumn is null)
                return Result.Failure(GeneralErrors.NotFound);

            targetBoardTasks = await boardTaskRepository.GetListByColumnIdAsync(targetColumn.Id, ct);
        }

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (request.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn)
            {
                await boardPositioningService.MoveTaskRangeToColumnAndSaveAsync(
                    columnBoardTasks,
                    targetBoardTasks,
                    request.TargetColumnId!.Value,
                    request.MovePlacement!.Value,
                    ct);
            }
            else if (columnBoardTasks.Count > 0)
            {
                boardTaskRepository.RemoveRange(columnBoardTasks);
            }

            columnRepository.Remove(column);

            var remainingColumns = boardColumns
                .Where(boardColumn => boardColumn.Id != request.ColumnId)
                .ToList();

            if (remainingColumns.Count > 0)
                await boardPositioningService.ApplyCurrentOrderAndSaveAsync(remainingColumns, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return Result.Success();
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
