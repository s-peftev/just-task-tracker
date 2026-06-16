using FluentValidation;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.BoardTasks;

public record DeleteBoardTaskCommand(Guid ColumnId, Guid BoardTaskId) : IRequest<Result>;

public class DeleteBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardTaskCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskCommand request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageTasks(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var columnBoardTasks = await boardTaskRepository.GetListByColumnIdAsync(request.ColumnId, ct);

        var boardTask = columnBoardTasks
            .FirstOrDefault(task => task.Id == request.BoardTaskId);

        if (boardTask is null)
            return Result.Failure(GeneralErrors.NotFound);

        var comments = await boardTaskCommentRepository.GetListByBoardTaskIdAsync(request.BoardTaskId, ct);
        
        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (comments.Count > 0)
                boardTaskCommentRepository.RemoveRange(comments);

            // we don't delete attachments from blob storage here so in case of recovering the task, the attachments are still available

            boardTaskRepository.Remove(boardTask);

            var remainingTasks = columnBoardTasks
                .Where(task => task.Id != boardTask.Id)
                .ToList();

            if (remainingTasks.Count > 0)
                await boardPositioningService.ApplyCurrentOrderAndSaveAsync(remainingTasks, ct);
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
        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();
    }
}
