using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Commands;

// AD-15: a trigger into the ACS Event Grid pipeline, not a second writer of CallSession.Status --
// removes the current participants from the Room; AD-12's webhook handlers then perform the
// actual close exactly as they would for a voluntary hang-up.
public record EndCallCommand(Guid CallSessionId) : IRequest<Result>;

public class EndCallCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    IAcsCallProvisioningService acsCallProvisioningService)
    : IRequestHandler<EndCallCommand, Result>
{
    public async Task<Result> Handle(EndCallCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByIdAsync(request.CallSessionId, ct);

        if (callSession is null)
            return Result.Failure(GeneralErrors.NotFound);

        var currentUser = await userRepository.GetUserByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var userRole = await boardRepository.GetUserRoleAsync(callSession.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        var isCreator = callSession.CreatedByUserId == currentUser.Id;
        var canEndAsRole = userRole is { } role && BoardRolePermissions.CanEndCall(role);

        if (!isCreator && !canEndAsRole)
            return Result.Failure(GeneralErrors.Forbidden);

        if (callSession.Status != CallStatus.Active)
            return Result.Failure(CallSessionsErrors.NotActive);

        var activeParticipants = await callParticipantRepository.GetActiveParticipantsAsync(callSession.Id, ct);
        var userIds = activeParticipants.Select(p => p.UserId).ToList();

        await acsCallProvisioningService.RemoveParticipantsAsync(callSession.AcsRoomId, userIds, ct);

        return Result.Success();
    }
}

public class EndCallCommandValidator : AbstractValidator<EndCallCommand>
{
    public EndCallCommandValidator()
    {
        RuleFor(x => x.CallSessionId)
            .NotEmpty();
    }
}
