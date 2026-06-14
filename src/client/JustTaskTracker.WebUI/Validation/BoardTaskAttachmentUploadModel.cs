namespace JustTaskTracker.WebUI.Validation;

public class BoardTaskAttachmentUploadModel
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int CurrentAttachmentCount { get; set; }
}
