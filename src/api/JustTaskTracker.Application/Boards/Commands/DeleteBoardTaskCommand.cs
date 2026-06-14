using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record DeleteBoardTaskCommand(Guid BoardId, Guid ColumnId, Guid BoardTaskId) : IRequest<Result>;

public class DeleteBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardTaskCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanManageTasks) is { } failure)
            return failure;

        var boardTask = await boardTaskRepository.GetByBoardIdAndIdAsync(
            request.BoardId,
            request.BoardTaskId,
            ct);

        if (boardTask is null || boardTask.ColumnId != request.ColumnId)
            return Result.Failure(GeneralErrors.NotFound);

        var comments = await boardTaskCommentRepository.GetOrderedByBoardTaskIdAsync(request.BoardTaskId, ct);
        var columnTasks = await boardTaskRepository.GetOrderedByColumnIdAsync(request.ColumnId, ct);

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (comments.Count > 0)
                boardTaskCommentRepository.RemoveRange(comments);

            boardTaskRepository.Remove(boardTask);

            var remainingTasks = columnTasks
                .Where(task => task.Id != boardTask.Id)
                .ToList();

            if (remainingTasks.Count > 0)
                await boardPositioningService.ApplyCurrentOrderAsync(remainingTasks, ct);
            else
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

public class DeleteBoardTaskCommandValidator : AbstractValidator<DeleteBoardTaskCommand>
{
    public DeleteBoardTaskCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();
    }
}
