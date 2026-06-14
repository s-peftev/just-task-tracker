using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
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

namespace JustTaskTracker.Application.Boards.Commands;

public record DeleteBoardTaskAttachmentCommand(Guid BoardId, Guid ColumnId, Guid BoardTaskId, Guid AttachmentId) : IRequest<Result>;

public class DeleteBoardTaskAttachmentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardPositioningService boardPositioningService,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBoardTaskAttachmentCommandHandler> logger)
    : IRequestHandler<DeleteBoardTaskAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskAttachmentCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanManageTasks) is { } failure)
            return failure;

        var boardTask = await boardTaskRepository.GetByBoardIdAndColumnIdAndIdWithAttachmentsAsync(
            request.BoardId,
            request.ColumnId,
            request.BoardTaskId,
            ct);

        if (boardTask is null)
            return Result.Failure(GeneralErrors.NotFound);

        var attachment = boardTask.Attachments
            .FirstOrDefault(a => a.Id == request.AttachmentId);

        if (attachment is null)
            return Result.Failure(GeneralErrors.NotFound);

        var blobName = attachment.BlobName;

        boardTask.Attachments.Remove(attachment);

        var remainingAttachments = boardTask.Attachments.ToList();

        if (remainingAttachments.Count > 0)
            await boardPositioningService.ApplyCurrentOrderAsync(remainingAttachments, ct);
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
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.AttachmentId)
            .NotEmpty();
    }
}
