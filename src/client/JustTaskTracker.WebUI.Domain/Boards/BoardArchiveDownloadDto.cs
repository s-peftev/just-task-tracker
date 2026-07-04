namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardArchiveDownloadDto(
    Uri DownloadUrl,
    DateTime DownloadUrlExpiresAtUtc,
    string FileName);
