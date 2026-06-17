using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class BoardMembersErrors
{
    public static readonly Error OwnerRoleNotAllowed = new(
        nameof(OwnerRoleNotAllowed),
        ErrorType.Validation,
        ["The Owner role cannot be assigned when adding a board member."]);

    public static readonly Error UserAlreadyMember = new(
        nameof(UserAlreadyMember),
        ErrorType.Business,
        ["The user is already a member of this board."]);

    public static readonly Error OwnerCannotBeRemoved = new(
        nameof(OwnerCannotBeRemoved),
        ErrorType.Business,
        ["The board owner cannot be removed."]);
}
