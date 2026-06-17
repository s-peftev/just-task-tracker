using FluentValidation;
using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Constants;
using JustTaskTracker.WebUI.Services.Configuration;

namespace JustTaskTracker.WebUI.Validation;

public class BoardTaskAttachmentUploadModelValidator : AbstractValidator<BoardTaskAttachmentUploadModel>
{
    public BoardTaskAttachmentUploadModelValidator(ValidationSettings validationSettings)
    {
        var boardTasks = validationSettings.BoardTasks;

        var allowedContentTypes = new HashSet<string>(
            boardTasks.AllowedContentTypes,
            StringComparer.OrdinalIgnoreCase);

        RuleFor(x => x.CurrentAttachmentCount)
            .LessThan(boardTasks.MaxAttachmentsPerTask)
            .WithMessage("The maximum number of attachments for this task has been reached.");

        RuleFor(x => x.FileName)
            .Must(fileName => !string.IsNullOrWhiteSpace(Path.GetFileName(fileName.Trim())))
            .WithMessage("A file must be selected.")
            .Must(fileName => Path.GetFileName(fileName.Trim()).Length <= BoardTaskAttachmentFieldLengths.MaxOriginalFileNameLength)
            .WithMessage($"File name must not exceed {BoardTaskAttachmentFieldLengths.MaxOriginalFileNameLength} characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("File type is not supported.")
            .Must(contentType => allowedContentTypes.Contains(contentType))
            .WithMessage("File type is not supported.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("The selected file is empty.")
            .LessThanOrEqualTo(boardTasks.MaxAttachmentSizeBytes)
            .WithMessage($"File size must not exceed {BoardTaskAttachmentValidationDisplay.FormatMaxFileSize(boardTasks.MaxAttachmentSizeBytes)}.");
    }
}
