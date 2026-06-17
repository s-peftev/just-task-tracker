using FluentValidation;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
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
    IBoardPositioningService boardPositioningService,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBoardTaskAttachmentCommandHandler> logger)
    : IRequestHandler<DeleteBoardTaskAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskAttachmentCommand request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageTasks(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var allAttachments = await boardTaskRepository.GetAttachmentsAsync(request.BoardTaskId, ct);

        var attachment = allAttachments
            .FirstOrDefault(a => a.Id == request.AttachmentId);

        if (attachment is null)
            return Result.Failure(GeneralErrors.NotFound);

        var blobName = attachment.BlobName;

        var remainingAttachments = allAttachments
            .Where(a => a.Id != request.AttachmentId)
            .ToList();

        boardTaskRepository.RemoveAttachment(attachment);

        if (remainingAttachments.Count > 0)
            await boardPositioningService.ApplyCurrentOrderAndSaveAsync(remainingAttachments, ct);
        else
            await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await blobStorageService.DeleteAsync(blobName, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Attachment {AttachmentId} removed from database but blob {BlobName} could not be deleted.",
                request.AttachmentId,
                blobName);
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
