namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportMemberDto(
    BoardExportUserDto User,
    string Role,
    DateTime JoinedAtUtc);
