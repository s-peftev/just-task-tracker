namespace JustTaskTracker.WebUI.Validation;

public class ProfilePhotoUploadModel
{
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
}
