using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class ColumnsErrors
{
    public static readonly Error DuplicateName = new(
        nameof(DuplicateName),
        ErrorType.Business,
        ["A column with this name already exists on the board."]);
}
