using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Attachments;

public record UploadBoardTaskAttachmentCommand(Guid BoardTaskId, IFormFile? File) : IRequest<Result<BoardTaskAttachmentDto>>;

public class UploadBoardTaskAttachmentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardTaskRepository boardTaskRepository,
    IBlobStorageService blobStorageService,
    ValidationSettings validationSettings,
    IUnitOfWork unitOfWork,
    ILogger<UploadBoardTaskAttachmentCommandHandler> logger)
    : IRequestHandler<UploadBoardTaskAttachmentCommand, Result<BoardTaskAttachmentDto>>
{
    public async Task<Result<BoardTaskAttachmentDto>> Handle(UploadBoardTaskAttachmentCommand request, CancellationToken ct)
    {
        var currentUser = await userRepository.GetUserDtoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.Unauthorized);

        var (boardTask, userRole) = await boardTaskRepository.GetBoardTaskWithUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageTasks(authorizedRole))
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.Forbidden);

        if (boardTask is null)
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.NotFound);

        var attachmentCount = await boardTaskRepository.GetAttachmentsCountAsync(request.BoardTaskId, ct);

        if (attachmentCount >= validationSettings.BoardTasks.MaxAttachmentsPerTask)
            return Result<BoardTaskAttachmentDto>.Failure(BoardTasksErrors.TooManyAttachments);

        var file = request.File!;
        var originalFileName = Path.GetFileName(file.FileName.Trim());
        var contentType = file.ContentType;
        var fileSizeBytes = file.Length;

        var blobName = $"{request.BoardTaskId}/{Guid.NewGuid()}";
        var position = attachmentCount;

        await using var stream = file.OpenReadStream();

        try
        {
            await blobStorageService.UploadAsync(blobName, stream, contentType, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload attachment blob {BlobName} for task {BoardTaskId}.", blobName, request.BoardTaskId);
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.ServiceUnavailable);
        }

        var attachment = new BoardTaskAttachment
        {
            BoardTaskId = boardTask.Id,
            UploadedById = currentUser.Id,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobName = blobName,
            Position = position,
        };

        boardTask.Attachments.Add(attachment);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to persist attachment metadata for blob {BlobName} on task {BoardTaskId}.",
                blobName,
                request.BoardTaskId);

            try
            {
                await blobStorageService.DeleteAsync(blobName, ct);
            }
            catch (Exception deleteEx)
            {
                logger.LogError(deleteEx, "Failed to delete orphaned blob {BlobName}.", blobName);
            }

            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.InternalServerError);
        }

        return Result<BoardTaskAttachmentDto>.Success(new BoardTaskAttachmentDto(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Position,
            attachment.CreatedAtUtc,
            currentUser));
    }
}

public class UploadBoardTaskAttachmentCommandValidator : AbstractValidator<UploadBoardTaskAttachmentCommand>
{
    public UploadBoardTaskAttachmentCommandValidator(ValidationSettings validationSettings)
    {
        var allowedContentTypes = new HashSet<string>(
            validationSettings.BoardTasks.AllowedContentTypes,
            StringComparer.OrdinalIgnoreCase);

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("'File' must not be empty.");

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File!.FileName)
                .Must(fileName => !string.IsNullOrWhiteSpace(Path.GetFileName(fileName.Trim())))
                .WithMessage("'File' must not be empty.")
                .Must(fileName => Path.GetFileName(fileName.Trim()).Length <= BoardTaskAttachmentFieldLengths.MaxOriginalFileNameLength)
                .WithMessage($"'File' name must be {BoardTaskAttachmentFieldLengths.MaxOriginalFileNameLength} characters or fewer.");

            RuleFor(x => x.File!.ContentType)
                .NotEmpty()
                .MaximumLength(BoardTaskAttachmentFieldLengths.MaxContentTypeLength)
                .Must(contentType => allowedContentTypes.Contains(contentType))
                .WithMessage("'ContentType' is not allowed.");

            RuleFor(x => x.File!.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(validationSettings.BoardTasks.MaxAttachmentSizeBytes);
        });
    }
}
