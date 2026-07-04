namespace JustTaskTracker.Domain.Boards.DTOs.Archiving;

public record BoardExportMemberDto(
    BoardExportUserDto User,
    string Role,
    DateTime JoinedAtUtc);
