using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Attachments;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Models;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Attachments;

public record DownloadBoardTaskAttachmentCommand(Guid AttachmentId) 
    : IRequest<Result<BoardTaskAttachmentDownload>>;

public class DownloadBoardTaskAttachmentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IAttachmentRepository attachmentRepository,
    IBoardTaskAttachmentService attachmentService,
    ILogger<DownloadBoardTaskAttachmentCommandHandler> logger)
    : IRequestHandler<DownloadBoardTaskAttachmentCommand, Result<BoardTaskAttachmentDownload>>
{
    public async Task<Result<BoardTaskAttachmentDownload>> Handle(
        DownloadBoardTaskAttachmentCommand request,
        CancellationToken ct)
    {
        var (attachment, userRole) = await attachmentRepository.GetAttachmentWithUserRoleAsync(request.AttachmentId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanDownloadAttachments(authorizedRole))
            return Result<BoardTaskAttachmentDownload>.Failure(GeneralErrors.Forbidden);

        if (attachment is null)
            return Result<BoardTaskAttachmentDownload>.Failure(GeneralErrors.NotFound);

        BlobContent blobContent;

        try
        {
            blobContent = await attachmentService.DownloadAsync(attachment.BlobName, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to download blob {BlobName} for attachment {AttachmentId}.",
                attachment.BlobName,
                request.AttachmentId);

            return Result<BoardTaskAttachmentDownload>.Failure(GeneralErrors.NotFound);
        }

        return Result<BoardTaskAttachmentDownload>.Success(
            new BoardTaskAttachmentDownload(attachment.OriginalFileName, blobContent));
    }
}

public class DownloadBoardTaskAttachmentCommandValidator : AbstractValidator<DownloadBoardTaskAttachmentCommand>
{
    public DownloadBoardTaskAttachmentCommandValidator()
    {
        RuleFor(x => x.AttachmentId)
            .NotEmpty();
    }
}
