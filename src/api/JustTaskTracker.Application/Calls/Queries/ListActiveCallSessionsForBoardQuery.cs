using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Enums;
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
    ICallParticipantRepository callParticipantRepository,
    ICallSessionAllowedParticipantRepository allowedParticipantRepository,
    ICallSessionLinkedTaskRepository linkedTaskRepository,
    IUserRepository userRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<ListActiveCallSessionsForBoardQuery, Result<IReadOnlyList<CallSessionDto>>>
{
    public async Task<Result<IReadOnlyList<CallSessionDto>>> Handle(ListActiveCallSessionsForBoardQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanJoinCall(role))
            return Result<IReadOnlyList<CallSessionDto>>.Failure(GeneralErrors.Forbidden);

        var sessions = await callRepository.GetActiveSessionsForBoardAsync(request.BoardId, ct);

        if (sessions.Count is 0)
            return Result<IReadOnlyList<CallSessionDto>>.Success([]);

        // AD-10 live-state UI (Story 3.2) needs participants/allowed users/linked tasks/creator for
        // every active session up front -- batched in a handful of queries keyed by session id,
        // never one round trip per session.
        var sessionIds = sessions.Select(s => s.Id).ToList();

        var participantsBySession = await callParticipantRepository.GetActiveParticipantUserIdsForSessionsAsync(sessionIds, ct);
        var allowedUserIdsBySession = await allowedParticipantRepository.GetAllowedUserIdsForSessionsAsync(sessionIds, ct);
        var linkedTasksBySession = await linkedTaskRepository.GetLinkedTaskLookupsForSessionsAsync(sessionIds, ct);

        var userIds = sessions.Select(s => s.CreatedByUserId)
            .Concat(participantsBySession.Values.SelectMany(ids => ids))
            .Concat(allowedUserIdsBySession.Values.SelectMany(ids => ids))
            .Distinct()
            .ToList();

        var usersById = await userRepository.GetUserInfoByIdsAsync(userIds, ct);

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var dtos = new List<CallSessionDto>(sessions.Count);

        foreach (var session in sessions)
        {
            // Defensive, not expected in practice: skip a row rather than fail the whole list if its
            // creator can no longer be resolved (matches the skip-defensively precedent used in
            // GetCallSessionHistoryForBoardQueryHandler).
            if (!usersById.TryGetValue(session.CreatedByUserId, out var createdByUser))
                continue;

            var participants = participantsBySession.GetValueOrDefault(session.Id, [])
                .Select(userId => usersById.GetValueOrDefault(userId))
                .Where(user => user is not null)
                .Select(user => user!.ToDto(profilePhotoUrlResolver))
                .ToList();

            // Client-side "can I join?" gate for Restricted sessions (AD-4) -- Open sessions don't need it.
            IReadOnlyList<UserDto>? allowedUsers = session.Visibility == CallVisibility.Restricted
                ? allowedUserIdsBySession.GetValueOrDefault(session.Id, [])
                    .Select(userId => usersById.GetValueOrDefault(userId))
                    .Where(user => user is not null)
                    .Select(user => user!.ToDto(profilePhotoUrlResolver))
                    .ToList()
                : null;

            var linkedTasks = linkedTasksBySession.GetValueOrDefault(session.Id, []);

            dtos.Add(new CallSessionDto(
                session.Id,
                session.BoardId,
                createdByUser.ToDto(profilePhotoUrlResolver),
                session.Title,
                session.Topic,
                session.Visibility,
                session.AcsRoomId,
                session.Status,
                session.StartedAtUtc,
                allowedUsers,
                linkedTasks,
                participants,
                session.CurrentPresenterUserId));
        }

        return Result<IReadOnlyList<CallSessionDto>>.Success(dtos);
    }
}
