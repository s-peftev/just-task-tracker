using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Entities;
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
    ICallRepository callRepository)
    : IRequestHandler<ListActiveCallSessionsForBoardQuery, Result<IReadOnlyList<CallSessionDto>>>
{
    public async Task<Result<IReadOnlyList<CallSessionDto>>> Handle(ListActiveCallSessionsForBoardQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanJoinCall(role))
            return Result<IReadOnlyList<CallSessionDto>>.Failure(GeneralErrors.Forbidden);

        var sessions = await callRepository.GetActiveSessionsForBoardAsync(request.BoardId, ct);

        return Result<IReadOnlyList<CallSessionDto>>.Success(sessions.Select(ToDto).ToList());
    }

    private static CallSessionDto ToDto(CallSession session) => new(
        session.Id,
        session.BoardId,
        session.CreatedByUserId,
        session.Title,
        session.Topic,
        session.Visibility,
        session.AcsRoomId,
        session.Status,
        session.StartedAtUtc);
}
