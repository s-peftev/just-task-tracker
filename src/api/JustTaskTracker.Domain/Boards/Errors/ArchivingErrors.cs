using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class ArchivingErrors
{
    public static readonly Error BoardNotEligibleForExport = new(
        nameof(BoardNotEligibleForExport),
        ErrorType.Conflict,
        ["The board is not archived or does not exist."]);
}
