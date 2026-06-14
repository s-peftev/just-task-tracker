using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Models;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands;

public record DownloadBoardTaskAttachmentCommand(
    Guid BoardId,
    Guid ColumnId,
    Guid BoardTaskId,
    Guid AttachmentId) : IRequest<Result<BoardTaskAttachmentDownload>>;

public class DownloadBoardTaskAttachmentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IBlobStorageService blobStorageService,
    ILogger<DownloadBoardTaskAttachmentCommandHandler> logger)
    : IRequestHandler<DownloadBoardTaskAttachmentCommand, Result<BoardTaskAttachmentDownload>>
{
    public async Task<Result<BoardTaskAttachmentDownload>> Handle(
        DownloadBoardTaskAttachmentCommand request,
        CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanDownloadAttachments) is { } failure)
            return Result<BoardTaskAttachmentDownload>.Failure(failure.Error);

        var attachment = await boardTaskRepository.GetAttachmentByBoardIdAndColumnIdAndTaskIdAsync(
            request.BoardId,
            request.ColumnId,
            request.BoardTaskId,
            request.AttachmentId,
            ct);

        if (attachment is null)
            return Result<BoardTaskAttachmentDownload>.Failure(GeneralErrors.NotFound);

        BlobContent blobContent;

        try
        {
            blobContent = await blobStorageService.DownloadAsync(attachment.BlobName, ct);
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
