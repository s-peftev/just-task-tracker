using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class ColumnsErrors
{
    public static readonly Error DuplicateName = new(
        nameof(DuplicateName),
        ErrorType.Business,
        ["A column with this name already exists on the board."]);

    public static readonly Error CannotMoveTasksToSameColumn = new(
        nameof(CannotMoveTasksToSameColumn),
        ErrorType.Business,
        ["Tasks cannot be moved to the column being deleted."]);

    public static readonly Error InvalidPosition = new(
        nameof(InvalidPosition),
        ErrorType.Validation,
        ["Column position is out of range."]);
}
