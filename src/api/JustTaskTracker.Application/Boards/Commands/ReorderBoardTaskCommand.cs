using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
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

namespace JustTaskTracker.Application.Boards.Commands;

public record ReorderBoardTaskCommand(Guid BoardId, Guid TargetColumnId, Guid BoardTaskId, int Position) : IRequest<Result>;

public class ReorderBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderBoardTaskCommand, Result>
{
    public async Task<Result> Handle(ReorderBoardTaskCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanMoveTasks) is { } failure)
            return failure;

        if (!await columnRepository.ExistsByBoardIdAndIdAsync(request.BoardId, request.TargetColumnId, ct))
            return Result.Failure(GeneralErrors.NotFound);

        var boardTask = await boardTaskRepository.GetByBoardIdAndIdAsync(request.BoardId, request.BoardTaskId, ct);

        if (boardTask is null)
            return Result.Failure(GeneralErrors.NotFound);

        var isSameColumnMove = boardTask.ColumnId == request.TargetColumnId;

        IReadOnlyList<BoardTask> columnTasks = [];
        IReadOnlyList<BoardTask> sourceTasks = [];
        IReadOnlyList<BoardTask> targetTasks = [];

        if (isSameColumnMove)
        {
            if (boardTask.Position == request.Position)
                return Result.Success();

            columnTasks = await boardTaskRepository.GetOrderedByColumnIdAsync(request.TargetColumnId, ct);

            if (request.Position >= columnTasks.Count)
                return Result.Failure(BoardTasksErrors.InvalidPosition);
        }
        else
        {
            targetTasks = await boardTaskRepository.GetOrderedByColumnIdAsync(request.TargetColumnId, ct);

            if (request.Position > targetTasks.Count)
                return Result.Failure(BoardTasksErrors.InvalidPosition);

            sourceTasks = await boardTaskRepository.GetOrderedByColumnIdAsync(boardTask.ColumnId, ct);
        }

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (isSameColumnMove)
            {
                await boardPositioningService.MoveToIndexAsync(columnTasks, request.BoardTaskId, request.Position, ct);
            }
            else
            {
                await boardPositioningService.MoveTaskToColumnAsync(
                    sourceTasks,
                    targetTasks,
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
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.TargetColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0);
    }
}
