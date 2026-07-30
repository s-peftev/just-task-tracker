using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Errors;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Domain.Calls.Notifications.Payloads;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Commands;

// AD-9: acquires the single-presenter lock via a conditional write (ICallRepository.TryAcquirePresenterAsync)
// before the client ever invokes the ACS SDK's local start-screen-share -- never the other way around.
public record RequestScreenShareCommand(Guid CallSessionId) : IRequest<Result>;

public class RequestScreenShareCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    ICallStateNotifier callStateNotifier)
    : IRequestHandler<RequestScreenShareCommand, Result>
{
    public async Task<Result> Handle(RequestScreenShareCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByIdAsync(request.CallSessionId, ct);

        if (callSession is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (callSession.Status != CallStatus.Active)
            return Result.Failure(CallSessionsErrors.NotActive);

        var currentUser = await userRepository.GetUserByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var activeParticipant = await callParticipantRepository.GetActiveParticipantAsync(callSession.Id, currentUser.Id, ct);

        if (activeParticipant is null)
            return Result.Failure(GeneralErrors.Forbidden);

        var acquired = await callRepository.TryAcquirePresenterAsync(callSession.Id, currentUser.Id, ct);

        if (!acquired)
            return Result.Failure(CallSessionsErrors.PresenterSlotTaken);

        await callStateNotifier.NotifyAsync(
            new CallStateNotification(
                callSession.BoardId,
                callSession.Id,
                CallStateNotificationType.PresenterChanged,
                new PresenterChangedPayload(currentUser.Id)),
            ct);

        return Result.Success();
    }
}

public class RequestScreenShareCommandValidator : AbstractValidator<RequestScreenShareCommand>
{
    public RequestScreenShareCommandValidator()
    {
        RuleFor(x => x.CallSessionId)
            .NotEmpty();
    }
}
