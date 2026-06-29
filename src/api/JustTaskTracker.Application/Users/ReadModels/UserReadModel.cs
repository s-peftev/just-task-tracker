namespace JustTaskTracker.Application.Users.ReadModels;

public record UserReadModel(
    Guid Id,
    string Email,
    string? DisplayName,
    string? ProfilePhotoVersion);
