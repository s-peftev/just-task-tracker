namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public sealed record BoardExportMemberDto(
    BoardExportUserDto User,
    string Role,
    DateTime JoinedAtUtc);
