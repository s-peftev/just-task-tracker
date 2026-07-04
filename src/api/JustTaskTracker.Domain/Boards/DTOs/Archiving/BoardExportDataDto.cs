using JustTaskTracker.Domain.Boards.DTOs.Boards;

namespace JustTaskTracker.Domain.Boards.DTOs.Archiving;

/// <summary>
/// Root payload returned to the Archival Function. Optional sections are null
/// when the corresponding export flag is false in <see cref="BoardExportOptions"/>.
/// </summary>
public record BoardExportDataDto(
    BoardExportBoardDto Board,
    BoardExportOptions AppliedOptions,
    DateTime ExportedAtUtc,
    IReadOnlyList<BoardExportColumnDto> Columns,
    IReadOnlyList<BoardExportMemberDto>? Members);
