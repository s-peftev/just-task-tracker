using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Attachments;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs.Attachments;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Attachments;

public record UploadBoardTaskAttachmentCommand(Guid BoardId, Guid BoardTaskId, IFormFile? File) : IRequest<Result<BoardTaskAttachmentDto>>, IRequireActiveBoard;

public class UploadBoardTaskAttachmentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardTaskRepository boardTaskRepository,
    IAttachmentRepository attachmentRepository,
    IBoardTaskAttachmentService attachmentService,
    ValidationSettings validationSettings,
    IUnitOfWork unitOfWork,
    IBoardActionNotifier boardActionNotifier,
    IDateTimeProvider dateTimeProvider,
    ILogger<UploadBoardTaskAttachmentCommandHandler> logger,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<UploadBoardTaskAttachmentCommand, Result<BoardTaskAttachmentDto>>
{
    public async Task<Result<BoardTaskAttachmentDto>> Handle(UploadBoardTaskAttachmentCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.Unauthorized);

        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageTasks(authorizedRole))
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.Forbidden);

        var attachmentCount = await attachmentRepository.GetCountByBoardTaskIdAsync(request.BoardTaskId, ct);

        if (attachmentCount >= validationSettings.BoardTasks!.MaxAttachmentsPerTask)
            return Result<BoardTaskAttachmentDto>.Failure(BoardTasksErrors.TooManyAttachments);

        var file = request.File!;
        var originalFileName = Path.GetFileName(file.FileName.Trim());
        var contentType = file.ContentType;
        var fileSizeBytes = file.Length;
        var blobName = attachmentService.BuildActiveBlobName(request.BoardTaskId, Guid.NewGuid());
        var position = attachmentCount;

        await using var stream = file.OpenReadStream();

        try
        {
            await attachmentService.UploadAsync(blobName, stream, contentType, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload attachment blob {BlobName} for task {BoardTaskId}.", blobName, request.BoardTaskId);
            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.ServiceUnavailable);
        }

        var attachment = new BoardTaskAttachment
        {
            BoardTaskId = request.BoardTaskId,
            UploadedById = currentUserInfo.Id,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobName = blobName,
            Position = position,
        };

        attachmentRepository.Add(attachment);

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
                await attachmentService.DeleteAsync(blobName, ct);
            }
            catch (Exception deleteEx)
            {
                logger.LogError(deleteEx, "Failed to delete orphaned blob {BlobName}.", blobName);
            }

            return Result<BoardTaskAttachmentDto>.Failure(GeneralErrors.InternalServerError);
        }

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var attachmentDto = new BoardTaskAttachmentDto(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Position,
            attachment.CreatedAtUtc,
            currentUserInfo.ToDto(profilePhotoUrlResolver));

        var attachmentsCount = await attachmentRepository.GetCountByBoardTaskIdAsync(request.BoardTaskId, ct);

        await boardActionNotifier.NotifyAsync(new BoardActionNotification(
            request.BoardId,
            BoardActionNotificationType.TaskAttachmentsCountChanged,
            currentUserInfo.Id,
            dateTimeProvider.UtcNow,
            new TaskAttachmentsCountChangedPayload(request.BoardTaskId, attachmentsCount)), ct);

        return Result<BoardTaskAttachmentDto>.Success(attachmentDto);
    }
}

public class UploadBoardTaskAttachmentCommandValidator : AbstractValidator<UploadBoardTaskAttachmentCommand>
{
    public UploadBoardTaskAttachmentCommandValidator(ValidationSettings validationSettings)
    {
        var allowedContentTypes = new HashSet<string>(
            validationSettings.BoardTasks!.AllowedContentTypes!,
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
                .LessThanOrEqualTo(validationSettings.BoardTasks!.MaxAttachmentSizeBytes);
        });
    }
}
