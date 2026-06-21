using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;

public class DeleteBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IAttachmentRepository attachmentRepository,
    IBlobStorageService blobStorageService,
    BlobStorageSettings blobStorageSettings,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBoardCommandHandler> logger)
    : IRequestHandler<DeleteBoardCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanDeleteBoard(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var attachments = await attachmentRepository.GetListByBoardIdAsync(request.BoardId, ct);

        var oldBlobNames = attachments.Select(a => a.BlobName).ToList();
        var newBlobNames = attachments.Select(a => blobStorageSettings.TaskAttachments.ToDeletedBlobName(a.BlobName)).ToList();

        var boardColumns = await columnRepository.GetListByBoardIdAsync(request.BoardId, ct);
        var boardTasks = await boardTaskRepository.GetListByBoardIdAsync(request.BoardId, ct);
        var boardComments = await boardTaskCommentRepository.GetListByBoardIdAsync(request.BoardId, ct);

        foreach (var (attachment, newName) in attachments.Zip(newBlobNames))
        {
            attachment.BlobName = newName;
            attachmentRepository.Remove(attachment);
        }

        boardRepository.Remove(board!);
        columnRepository.RemoveRange(boardColumns);
        boardTaskRepository.RemoveRange(boardTasks);
        boardTaskCommentRepository.RemoveRange(boardComments);

        await unitOfWork.SaveChangesAsync(ct);

        foreach (var (oldName, newName) in oldBlobNames.Zip(newBlobNames))
        {
            try
            {
                await blobStorageService.MoveToDeletedAsync(oldName, newName, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Board {BoardId} deleted but attachment blob {BlobName} could not be moved to deleted storage.",
                    request.BoardId,
                    oldName);
            }
        }

        return Result.Success();
    }
}
