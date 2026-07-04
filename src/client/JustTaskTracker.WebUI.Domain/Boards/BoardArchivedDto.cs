using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardArchivedDto(
    DateTime ArchivedAtUtc,
    BoardExportStatus BoardExportStatus);
