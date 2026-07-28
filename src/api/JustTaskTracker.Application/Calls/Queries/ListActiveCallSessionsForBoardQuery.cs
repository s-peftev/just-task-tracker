using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Mappings;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Queries;

// FR13: point-in-time list, no real-time push required -- how anyone besides the creator
// discovers and joins an active call in Story 1.1.
public record ListActiveCallSessionsForBoardQuery(Guid BoardId) : IRequest<Result<IReadOnlyList<CallSessionDto>>>;

public class ListActiveCallSessionsForBoardQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<ListActiveCallSessionsForBoardQuery, Result<IReadOnlyList<CallSessionDto>>>
{
    public async Task<Result<IReadOnlyList<CallSessionDto>>> Handle(ListActiveCallSessionsForBoardQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanJoinCall(role))
            return Result<IReadOnlyList<CallSessionDto>>.Failure(GeneralErrors.Forbidden);

        // AD-10 live-state UI (Story 3.2) needs participants/allowed users/linked tasks/creator for
        // every active session -- projected via CallSession's navigation properties in one query
        // (CallRepository.GetActiveSessionsWithStateForBoardAsync), never one round trip per session.
        var sessions = await callRepository.GetActiveSessionsWithStateForBoardAsync(request.BoardId, ct);

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        IReadOnlyList<CallSessionDto> dtos = sessions.Select(session => session.ToDto(profilePhotoUrlResolver)).ToList();

        return Result<IReadOnlyList<CallSessionDto>>.Success(dtos);
    }
}
