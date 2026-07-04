namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportUserDto(
    Guid Id,
    string Email,
    string? DisplayName);
