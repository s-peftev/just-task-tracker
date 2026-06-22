using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class BoardMembersErrors
{
    public static readonly Error OwnerRoleNotAllowed = new(
        nameof(OwnerRoleNotAllowed),
        ErrorType.Validation,
        ["The Owner role cannot be assigned to a board member."]);

    public static readonly Error OwnerRoleCannotBeChanged = new(
        nameof(OwnerRoleCannotBeChanged),
        ErrorType.Business,
        ["The board owner's role cannot be changed."]);

    public static readonly Error UserAlreadyMember = new(
        nameof(UserAlreadyMember),
        ErrorType.Business,
        ["The user is already a member of this board."]);

    public static readonly Error OwnerCannotBeRemoved = new(
        nameof(OwnerCannotBeRemoved),
        ErrorType.Business,
        ["The board owner cannot be removed."]);

    public static readonly Error OwnerCannotLeaveBoard = new(
        nameof(OwnerCannotLeaveBoard),
        ErrorType.Business,
        ["The board owner cannot leave the board."]);

    public static readonly Error CannotChangeOwnRole = new(
        nameof(CannotChangeOwnRole),
        ErrorType.Business,
        ["You cannot change your own board role."]);
}
