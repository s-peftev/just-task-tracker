using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Calls.Errors;

public static class CallSessionsErrors
{
    public static readonly Error NotActive = new(
        nameof(NotActive), ErrorType.Conflict, ["This call session is no longer active."]);
}
