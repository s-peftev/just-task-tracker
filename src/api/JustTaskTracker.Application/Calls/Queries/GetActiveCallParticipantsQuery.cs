using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Queries;

// Backs the in-call participant tiles (name/email/avatar): the same CallParticipant rows Story 1.2's
// Event-Grid pipeline maintains, enriched with User info and each participant's AcsCommunicationUserId
// so the client can correlate a tile (keyed by its ACS raw id) back to a person.
public record GetActiveCallParticipantsQuery(Guid CallSessionId) : IRequest<Result<IReadOnlyList<CallParticipantDto>>>;

public class GetActiveCallParticipantsQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    IUserRepository userRepository,
    IAcsUserIdentityMappingRepository mappingRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetActiveCallParticipantsQuery, Result<IReadOnlyList<CallParticipantDto>>>
{
    public async Task<Result<IReadOnlyList<CallParticipantDto>>> Handle(GetActiveCallParticipantsQuery request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByIdAsync(request.CallSessionId, ct);

        if (callSession is null)
            return Result<IReadOnlyList<CallParticipantDto>>.Failure(GeneralErrors.NotFound);

        var userRole = await boardRepository.GetUserRoleAsync(callSession.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanJoinCall(role))
            return Result<IReadOnlyList<CallParticipantDto>>.Failure(GeneralErrors.Forbidden);

        var activeParticipants = await callParticipantRepository.GetActiveParticipantsAsync(callSession.Id, ct);

        var participants = new List<CallParticipantDto>(activeParticipants.Count);

        foreach (var participant in activeParticipants)
        {
            var user = await userRepository.GetByIdAsync(participant.UserId, ct);
            var mapping = await mappingRepository.GetByUserIdAsync(participant.UserId, ct);

            // Both should always exist for a recorded participant; skip defensively rather than fail the whole roster.
            if (user is null || mapping is null)
                continue;

            var profilePhotoUrl = user.ProfilePhotoVersion is null
                ? null
                : profilePhotoService.BuildOriginalUrl(user.Id, user.ProfilePhotoVersion);

            participants.Add(new CallParticipantDto(
                user.Id,
                mapping.AcsCommunicationUserId,
                user.DisplayName,
                user.Email,
                profilePhotoUrl,
                participant.JoinedAtUtc));
        }

        return Result<IReadOnlyList<CallParticipantDto>>.Success(participants);
    }
}

public class GetActiveCallParticipantsQueryValidator : AbstractValidator<GetActiveCallParticipantsQuery>
{
    public GetActiveCallParticipantsQueryValidator()
    {
        RuleFor(x => x.CallSessionId)
            .NotEmpty();
    }
}
