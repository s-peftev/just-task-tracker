using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Attachments;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Attachments;

public record DeleteBoardTaskAttachmentCommand(Guid BoardTaskId, Guid AttachmentId) : IRequest<Result>;

public class DeleteBoardTaskAttachmentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IAttachmentRepository attachmentRepository,
    IBoardPositioningService boardPositioningService,
    IBoardTaskAttachmentService attachmentService,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBoardTaskAttachmentCommandHandler> logger)
    : IRequestHandler<DeleteBoardTaskAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskAttachmentCommand request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageTasks(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var allAttachments = await attachmentRepository.GetListByBoardTaskIdAsync(request.BoardTaskId, ct);

        var attachment = allAttachments
            .FirstOrDefault(a => a.Id == request.AttachmentId);

        if (attachment is null)
            return Result.Failure(GeneralErrors.NotFound);

        var oldBlobName = attachment.BlobName;
        var newBlobName = attachmentService.ToDeletedBlobName(oldBlobName);

        var remainingAttachments = allAttachments
            .Where(a => a.Id != request.AttachmentId)
            .ToList();

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            attachmentRepository.Remove(attachment);
            attachment.BlobName = newBlobName;

            if (remainingAttachments.Count > 0)
                await boardPositioningService.ApplyCurrentOrderAndSaveAsync(remainingAttachments, ct);
            else
                await unitOfWork.SaveChangesAsync(ct);

            await unitOfWork.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(ct);

            logger.LogError(ex, "Failed to delete attachment {AttachmentId}.", request.AttachmentId);

            return Result.Failure(GeneralErrors.InternalServerError);
        }

        try
        {
            await attachmentService.MoveToDeletedAsync(oldBlobName, newBlobName, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Attachment {AttachmentId} soft-deleted but blob {OldBlobName} could not be moved to deleted storage.",
                request.AttachmentId,
                oldBlobName);
        }

        return Result.Success();
    }
}

public class DeleteBoardTaskAttachmentCommandValidator : AbstractValidator<DeleteBoardTaskAttachmentCommand>
{
    public DeleteBoardTaskAttachmentCommandValidator()
    {
        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.AttachmentId)
            .NotEmpty();
    }
}
