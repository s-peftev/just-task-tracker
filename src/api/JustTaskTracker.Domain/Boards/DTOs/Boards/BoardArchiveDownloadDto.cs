namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardArchiveDownloadDto(
    Uri DownloadUrl,
    DateTime DownloadUrlExpiresAtUtc,
    string FileName);
