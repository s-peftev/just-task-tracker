using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Attachments;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.BoardTasks;

public record DeleteBoardTaskCommand(Guid BoardId, Guid ColumnId, Guid BoardTaskId)
    : IRequest<Result>, IRequireActiveBoard;

public class DeleteBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IAttachmentRepository attachmentRepository,
    IBoardPositioningService boardPositioningService,
    IBoardTaskAttachmentService attachmentService,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBoardTaskCommandHandler> logger)
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
        var attachments = await attachmentRepository.GetListByBoardTaskIdAsync(request.BoardTaskId, ct);

        var oldBlobNames = attachments.Select(a => a.BlobName).ToList();
        var newBlobNames = attachments.Select(a => attachmentService.ToDeletedBlobName(a.BlobName)).ToList();

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            if (comments.Count > 0)
                boardTaskCommentRepository.RemoveRange(comments);

            foreach (var (attachment, newName) in attachments.Zip(newBlobNames))
            {
                attachment.BlobName = newName;
                attachmentRepository.Remove(attachment);
            }

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

        foreach (var (oldName, newName) in oldBlobNames.Zip(newBlobNames))
        {
            try
            {
                await attachmentService.MoveToDeletedAsync(oldName, newName, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Task {BoardTaskId} deleted but attachment blob {BlobName} could not be moved to deleted storage.",
                    request.BoardTaskId,
                    oldName);
            }
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
