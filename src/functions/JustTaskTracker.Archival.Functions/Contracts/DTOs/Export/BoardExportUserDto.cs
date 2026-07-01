namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public sealed record BoardExportUserDto(
    Guid Id,
    string Email,
    string? DisplayName);
