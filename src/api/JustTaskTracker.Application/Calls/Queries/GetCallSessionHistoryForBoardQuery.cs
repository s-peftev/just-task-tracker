using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Mappings;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Queries;

public record GetCallSessionHistoryForBoardQuery(Guid BoardId) : PaginatedRequest, IRequest<Result<PagedList<CallSessionHistoryDto>>>;

public class GetCallSessionHistoryForBoardQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    ICallSessionAllowedParticipantRepository allowedParticipantRepository,
    ICallSessionLinkedTaskRepository linkedTaskRepository,
    IUserRepository userRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetCallSessionHistoryForBoardQuery, Result<PagedList<CallSessionHistoryDto>>>
{
    public async Task<Result<PagedList<CallSessionHistoryDto>>> Handle(GetCallSessionHistoryForBoardQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanViewBoard(role))
            return Result<PagedList<CallSessionHistoryDto>>.Failure(GeneralErrors.Forbidden);

        var sessions = await callRepository.GetClosedSessionsForBoardAsync(
            request.BoardId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            ct);

        // Batched (keyed by session id) instead of one round trip per session/per user -- same
        // approach as ListActiveCallSessionsForBoardQueryHandler. The batch repository methods
        // already short-circuit on an empty id list, so no separate empty-page check is needed.
        var sessionIds = sessions.Items.Select(s => s.Id).ToList();

        var linkedTasksBySession = await linkedTaskRepository.GetLinkedTaskLookupsForSessionsAsync(sessionIds, ct);
        var participantHistoryBySession = await callParticipantRepository.GetParticipantHistoryForSessionsAsync(sessionIds, ct);
        var allowedUserIdsBySession = await allowedParticipantRepository.GetAllowedUserIdsForSessionsAsync(sessionIds, ct);

        var userIds = sessions.Items.Select(s => s.CreatedByUserId)
            .Concat(participantHistoryBySession.Values.SelectMany(entries => entries.Select(e => e.UserId)))
            .Concat(allowedUserIdsBySession.Values.SelectMany(ids => ids))
            .Distinct()
            .ToList();

        var usersById = await userRepository.GetUserInfoByIdsAsync(userIds, ct);

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var items = new List<CallSessionHistoryDto>();

        foreach (var session in sessions.Items)
        {
            // Defensive, not expected in practice: skip a row rather than fail the whole page if its
            // creator can no longer be resolved (matches the skip-defensively precedent used for
            // participant tiles in GetActiveCallParticipantsQueryHandler).
            if (!usersById.TryGetValue(session.CreatedByUserId, out var createdByUser))
                continue;

            var linkedTasks = linkedTasksBySession.GetValueOrDefault(session.Id, []);

            var participantHistory = participantHistoryBySession.GetValueOrDefault(session.Id, []);
            var participants = new List<CallHistoryParticipantDto>(participantHistory.Count);

            foreach (var entry in participantHistory)
            {
                if (usersById.TryGetValue(entry.UserId, out var participantUser))
                    participants.Add(entry.ToDto(participantUser.ToDto(profilePhotoUrlResolver)));
            }

            IReadOnlyList<UserDto>? allowedUsers = null;

            if (session.Visibility == CallVisibility.Restricted)
            {
                allowedUsers = allowedUserIdsBySession.GetValueOrDefault(session.Id, [])
                    .Select(userId => usersById.GetValueOrDefault(userId))
                    .Where(user => user is not null)
                    .Select(user => user!.ToDto(profilePhotoUrlResolver))
                    .ToList();
            }

            items.Add(session.ToHistoryDto(createdByUser.ToDto(profilePhotoUrlResolver), linkedTasks, participants, allowedUsers));
        }

        return Result<PagedList<CallSessionHistoryDto>>.Success(new PagedList<CallSessionHistoryDto>(sessions.Metadata, items));
    }
}

public class GetCallSessionHistoryForBoardQueryValidator : AbstractValidator<GetCallSessionHistoryForBoardQuery>
{
    public GetCallSessionHistoryForBoardQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();
    }
}
