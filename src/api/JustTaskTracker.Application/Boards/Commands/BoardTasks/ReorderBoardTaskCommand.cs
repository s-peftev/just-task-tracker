using FluentValidation;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.BoardTasks;

public record ReorderBoardTaskCommand(Guid TargetColumnId, Guid BoardTaskId, int Position) : IRequest<Result>;

public class ReorderBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderBoardTaskCommand, Result>
{
    public async Task<Result> Handle(ReorderBoardTaskCommand request, CancellationToken ct)
    {
        var (boardTask, userRole) = await boardTaskRepository.GetBoardTaskWithUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanMoveTasks(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        if (boardTask is null)
            return Result.Failure(GeneralErrors.NotFound);

        var isSameColumnMove = boardTask.ColumnId == request.TargetColumnId;

        IReadOnlyList<BoardTask> columnTasks = [];
        IReadOnlyList<BoardTask> sourceBoardTasks = [];
        IReadOnlyList<BoardTask> targetBoardTasks = [];

        if (isSameColumnMove)
        {
            if (boardTask.Position == request.Position)
                return Result.Success();

            columnTasks = await boardTaskRepository.GetListByColumnIdAsync(request.TargetColumnId, ct);

            if (request.Position >= columnTasks.Count)
                return Result.Failure(BoardTasksErrors.InvalidPosition);
        }
        else
        {
            targetBoardTasks = await boardTaskRepository.GetListByColumnIdAsync(request.TargetColumnId, ct);

            if (request.Position > targetBoardTasks.Count)
                return Result.Failure(BoardTasksErrors.InvalidPosition);

            sourceBoardTasks = await boardTaskRepository.GetListByColumnIdAsync(boardTask.ColumnId, ct);
        }

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (isSameColumnMove)
            {
                await boardPositioningService.MoveToIndexAndSaveAsync(columnTasks, request.BoardTaskId, request.Position, ct);
            }
            else
            {
                await boardPositioningService.MoveTaskToColumnAndSaveAsync(
                    sourceBoardTasks,
                    targetBoardTasks,
                    boardTask,
                    request.TargetColumnId,
                    request.Position,
                    ct);
            }

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

public class ReorderBoardTaskCommandValidator : AbstractValidator<ReorderBoardTaskCommand>
{
    public ReorderBoardTaskCommandValidator()
    {
        RuleFor(x => x.TargetColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0);
    }
}
