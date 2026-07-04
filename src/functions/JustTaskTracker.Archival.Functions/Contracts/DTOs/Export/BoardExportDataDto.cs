using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

/// <summary>
/// Root payload for archive generation. Optional sections are null when the corresponding export flag is false.
/// </summary>
public record BoardExportDataDto(
    BoardExportBoardDto Board,
    BoardExportOptions AppliedOptions,
    DateTime ExportedAtUtc,
    IReadOnlyList<BoardExportColumnDto> Columns,
    IReadOnlyList<BoardExportMemberDto>? Members);
