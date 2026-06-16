namespace JustTaskTracker.WebUI.Services.Configuration;

public class ValidationSettings
{
    public const string SectionName = "ValidationSettings";

    public BoardTaskValidationSettings BoardTasks { get; set; } = new();
}

public class BoardTaskValidationSettings
{
    private const long DefaultMaxAttachmentSizeBytes = 10 * 1024 * 1024;

    private const int DefaultMaxAttachmentsPerTask = 10;

    private static readonly string[] DefaultAllowedContentTypes =
    [
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/gif",
        "image/webp",
        "text/plain",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/zip",
    ];

    public long MaxAttachmentSizeBytes { get; set; } = DefaultMaxAttachmentSizeBytes;

    public int MaxAttachmentsPerTask { get; set; } = DefaultMaxAttachmentsPerTask;

    public string[] AllowedContentTypes { get; set; } = DefaultAllowedContentTypes.ToArray();
}
