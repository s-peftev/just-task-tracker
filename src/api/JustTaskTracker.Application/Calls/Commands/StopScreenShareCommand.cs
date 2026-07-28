using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Errors;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Domain.Calls.Notifications.Payloads;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Commands;

// AD-9: releases the single-presenter lock via a conditional write -- only succeeds if the
// caller is the one currently holding it.
public record StopScreenShareCommand(Guid CallSessionId) : IRequest<Result>;

public class StopScreenShareCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    ICallRepository callRepository,
    ICallStateNotifier callStateNotifier)
    : IRequestHandler<StopScreenShareCommand, Result>
{
    public async Task<Result> Handle(StopScreenShareCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByIdAsync(request.CallSessionId, ct);

        if (callSession is null)
            return Result.Failure(GeneralErrors.NotFound);

        var currentUser = await userRepository.GetUserByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var released = await callRepository.TryReleasePresenterAsync(callSession.Id, currentUser.Id, ct);

        if (!released)
            return Result.Failure(CallSessionsErrors.NotPresenter);

        await callStateNotifier.NotifyAsync(
            new CallStateNotification(
                callSession.BoardId,
                callSession.Id,
                CallStateNotificationType.PresenterChanged,
                new PresenterChangedPayload(null)),
            ct);

        return Result.Success();
    }
}

public class StopScreenShareCommandValidator : AbstractValidator<StopScreenShareCommand>
{
    public StopScreenShareCommandValidator()
    {
        RuleFor(x => x.CallSessionId)
            .NotEmpty();
    }
}
