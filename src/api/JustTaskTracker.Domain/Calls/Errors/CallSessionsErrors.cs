using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Calls.Errors;

public static class CallSessionsErrors
{
    public static readonly Error NotActive = new(
        nameof(NotActive), ErrorType.Conflict, ["This call session is no longer active."]);

    public static readonly Error AllowedParticipantNotBoardMember = new(
        nameof(AllowedParticipantNotBoardMember),
        ErrorType.Business,
        ["A restricted call's allow-list can only include members of this board."]);

    public static readonly Error LinkedTaskNotOnBoard = new(
        nameof(LinkedTaskNotOnBoard),
        ErrorType.Business,
        ["A call can only link tasks that belong to its own board."]);
}
