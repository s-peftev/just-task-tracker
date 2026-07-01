using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class BoardsErrors
{
    public static readonly Error Archived = new(
        nameof(Archived),
        ErrorType.Conflict,
        ["This board is archived and cannot be modified."]);
}
