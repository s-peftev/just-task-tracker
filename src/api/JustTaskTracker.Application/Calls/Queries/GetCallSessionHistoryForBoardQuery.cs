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

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var items = new List<CallSessionHistoryDto>();

        foreach (var session in sessions.Items)
        {
            // Defensive, not expected in practice: skip a row rather than fail the whole page if its
            // creator can no longer be resolved (matches the skip-defensively precedent used for
            // participant tiles in GetActiveCallParticipantsQueryHandler).
            if (await ResolveUserDtoAsync(session.CreatedByUserId, profilePhotoUrlResolver, ct) is not { } createdBy)
                continue;

            var linkedTasks = await linkedTaskRepository.GetLinkedTaskLookupsAsync(session.Id, ct);

            var participantHistory = await callParticipantRepository.GetParticipantHistoryAsync(session.Id, ct);
            var participants = new List<CallHistoryParticipantDto>(participantHistory.Count);

            foreach (var entry in participantHistory)
            {
                if (await ResolveUserDtoAsync(entry.UserId, profilePhotoUrlResolver, ct) is { } participantUser)
                    participants.Add(entry.ToDto(participantUser));
            }

            IReadOnlyList<UserDto>? allowedUsers = null;

            if (session.Visibility == CallVisibility.Restricted)
            {
                var allowedUserIds = await allowedParticipantRepository.GetAllowedUserIdsAsync(session.Id, ct);
                var resolvedAllowedUsers = new List<UserDto>(allowedUserIds.Count);

                foreach (var userId in allowedUserIds)
                {
                    if (await ResolveUserDtoAsync(userId, profilePhotoUrlResolver, ct) is { } allowedUser)
                        resolvedAllowedUsers.Add(allowedUser);
                }

                allowedUsers = resolvedAllowedUsers;
            }

            items.Add(session.ToHistoryDto(createdBy, linkedTasks, participants, allowedUsers));
        }

        return Result<PagedList<CallSessionHistoryDto>>.Success(new PagedList<CallSessionHistoryDto>(sessions.Metadata, items));
    }

    private async Task<UserDto?> ResolveUserDtoAsync(Guid userId, Func<UserReadModel, string?> profilePhotoUrlResolver, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);

        return user is null
            ? null
            : new UserReadModel(user.Id, user.Email, user.DisplayName, user.ProfilePhotoVersion).ToDto(profilePhotoUrlResolver);
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
