using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Domain.Auth.Constants;
using JustTaskTracker.Domain.Boards.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class ValidationSettings
{
    public BoardValidationSettings? Boards { get; set; }

    public BoardTaskValidationSettings? BoardTasks { get; set; }

    public UserValidationSettings? Users { get; set; }

    public ProfilePhotoValidationSettings? ProfilePhotos { get; set; }

    public void Validate()
    {
        var section = ConfigSections.ValidationSettings;

        if (Boards is null)
            throw new InvalidOperationException($"{section}:Boards is not configured.");

        if (BoardTasks is null)
            throw new InvalidOperationException($"{section}:BoardTasks is not configured.");

        if (Users is null)
            throw new InvalidOperationException($"{section}:Users is not configured.");

        if (ProfilePhotos is null)
            throw new InvalidOperationException($"{section}:ProfilePhotos is not configured.");

        Boards.Validate($"{section}:Boards");
        BoardTasks.Validate($"{section}:BoardTasks");
        Users.Validate($"{section}:Users");
        ProfilePhotos.Validate($"{section}:ProfilePhotos");
    }
}

public class BoardValidationSettings
{
    public int MaxBoardNameSearchLength { get; set; }

    internal void Validate(string sectionPath)
    {
        if (MaxBoardNameSearchLength == 0)
            throw new InvalidOperationException($"{sectionPath}:MaxBoardNameSearchLength is not configured.");

        if (MaxBoardNameSearchLength < 0)
            throw new InvalidOperationException($"{sectionPath}:MaxBoardNameSearchLength must be greater than 0.");

        if (MaxBoardNameSearchLength > BoardFieldLengths.MaxNameLength)
            throw new InvalidOperationException(
                $"{sectionPath}:MaxBoardNameSearchLength must not exceed {BoardFieldLengths.MaxNameLength}.");
    }
}

public class BoardTaskValidationSettings
{
    public long MaxAttachmentSizeBytes { get; set; }

    public int MaxAttachmentsPerTask { get; set; }

    public string[]? AllowedContentTypes { get; set; }

    public int MaxTextSearchLength { get; set; }

    internal void Validate(string sectionPath)
    {
        if (MaxAttachmentSizeBytes == 0)
            throw new InvalidOperationException($"{sectionPath}:MaxAttachmentSizeBytes is not configured.");

        if (MaxAttachmentSizeBytes < 0)
            throw new InvalidOperationException($"{sectionPath}:MaxAttachmentSizeBytes must be greater than 0.");

        if (MaxAttachmentsPerTask == 0)
            throw new InvalidOperationException($"{sectionPath}:MaxAttachmentsPerTask is not configured.");

        if (MaxAttachmentsPerTask < 0)
            throw new InvalidOperationException($"{sectionPath}:MaxAttachmentsPerTask must be greater than 0.");

        if (AllowedContentTypes is null || AllowedContentTypes.Length == 0)
            throw new InvalidOperationException($"{sectionPath}:AllowedContentTypes is not configured.");

        if (AllowedContentTypes.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException($"{sectionPath}:AllowedContentTypes must not contain empty values.");

        foreach (var contentType in AllowedContentTypes)
        {
            if (contentType.Length > BoardTaskAttachmentFieldLengths.MaxContentTypeLength)
                throw new InvalidOperationException(
                    $"{sectionPath}:AllowedContentTypes entries must not exceed {BoardTaskAttachmentFieldLengths.MaxContentTypeLength} characters.");
        }

        if (MaxTextSearchLength == 0)
            throw new InvalidOperationException($"{sectionPath}:MaxTextSearchLength is not configured.");

        if (MaxTextSearchLength < 0)
            throw new InvalidOperationException($"{sectionPath}:MaxTextSearchLength must be greater than 0.");

        if (MaxTextSearchLength > BoardTaskFieldLengths.MaxDescriptionLength)
            throw new InvalidOperationException(
                $"{sectionPath}:MaxTextSearchLength must not exceed {BoardTaskFieldLengths.MaxDescriptionLength}.");
    }
}

public class UserValidationSettings
{
    public int MaxTextSearchLength { get; set; }

    internal void Validate(string sectionPath)
    {
        if (MaxTextSearchLength == 0)
            throw new InvalidOperationException($"{sectionPath}:MaxTextSearchLength is not configured.");

        if (MaxTextSearchLength < 0)
            throw new InvalidOperationException($"{sectionPath}:MaxTextSearchLength must be greater than 0.");

        if (MaxTextSearchLength > UserFieldLengths.MaxEmailLength)
            throw new InvalidOperationException(
                $"{sectionPath}:MaxTextSearchLength must not exceed {UserFieldLengths.MaxEmailLength}.");
    }
}

public class ProfilePhotoValidationSettings
{
    public long MaxPhotoSizeBytes { get; set; }

    public string[]? AllowedContentTypes { get; set; }

    internal void Validate(string sectionPath)
    {
        if (MaxPhotoSizeBytes == 0)
            throw new InvalidOperationException($"{sectionPath}:MaxPhotoSizeBytes is not configured.");

        if (MaxPhotoSizeBytes < 0)
            throw new InvalidOperationException($"{sectionPath}:MaxPhotoSizeBytes must be greater than 0.");

        if (AllowedContentTypes is null || AllowedContentTypes.Length == 0)
            throw new InvalidOperationException($"{sectionPath}:AllowedContentTypes is not configured.");

        if (AllowedContentTypes.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException($"{sectionPath}:AllowedContentTypes must not contain empty values.");
    }
}
