using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;

namespace JustTaskTracker.Application.Boards.Authorization;

public static class BoardRoleAuthorization
{
    public static Result? EnsureBoardAccess(bool exists, BoardMemberRole? role, Func<BoardMemberRole, bool> can)
    {
        if (!exists)
            return Result.Failure(GeneralErrors.NotFound);

        if (role is not { } authorizedRole || !can(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        return null;
    }
}
