using FluentValidation;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Domain.Calls.Notifications.Payloads;
using JustTaskTracker.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Calls.Commands.Internal;

// Invoked only from CallsWebhookController on Microsoft.Communication.CallEnded (AD-12). This is a
// call-level event (no participant identity in its payload) and is authoritative for closing a
// session independently of RecordParticipantLeftCommand's own "last participant left" check --
// whichever of the two arrives first closes the session; the other is then a no-op.
public record RecordCallEndedCommand(string AcsRoomId, DateTime OccurredAtUtc) : IRequest<Result>;

public class RecordCallEndedCommandHandler(
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    ICallStateNotifier callStateNotifier,
    IAcsCallProvisioningService acsCallProvisioningService,
    IUnitOfWork unitOfWork,
    ILogger<RecordCallEndedCommandHandler> logger)
    : IRequestHandler<RecordCallEndedCommand, Result>
{
    public async Task<Result> Handle(RecordCallEndedCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByAcsRoomIdAsync(request.AcsRoomId, ct);

        if (callSession is null)
        {
            logger.LogWarning("Ignoring CallEnded for unknown ACS room {AcsRoomId}.", request.AcsRoomId);
            return Result.Success();
        }

        if (callSession.Status == CallStatus.Closed)
            return Result.Success(); // idempotent: already closed, by an earlier CallEnded or the last participant's departure

        // The call is confirmed over -- any row still without a LeftAtUtc must be backfilled now,
        // since its own CallParticipantRemoved may have been lost or may never arrive.
        var activeParticipants = await callParticipantRepository.GetActiveParticipantsAsync(callSession.Id, ct);

        foreach (var participant in activeParticipants)
            participant.LeftAtUtc = request.OccurredAtUtc;

        callSession.Status = CallStatus.Closed;
        callSession.EndedAtUtc = request.OccurredAtUtc;
        callSession.CurrentPresenterUserId = null;

        await unitOfWork.SaveChangesAsync(ct);

        await callStateNotifier.NotifyAsync(new CallStateNotification(
            callSession.BoardId,
            callSession.Id,
            CallStateNotificationType.SessionClosed,
            new SessionClosedPayload(request.OccurredAtUtc)), ct);

        // One fresh Room per session, never reused (AD-8) -- safe to delete now that the
        // session is authoritatively closed. Best-effort: a failed cleanup here doesn't
        // affect the already-committed session state.
        try
        {
            await acsCallProvisioningService.DeleteRoomAsync(callSession.AcsRoomId, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to delete ACS Room {AcsRoomId} after call session {CallSessionId} closed.",
                callSession.AcsRoomId,
                callSession.Id);
        }

        return Result.Success();
    }
}

public class RecordCallEndedCommandValidator : AbstractValidator<RecordCallEndedCommand>
{
    public RecordCallEndedCommandValidator()
    {
        RuleFor(x => x.AcsRoomId)
            .Must(roomId => !string.IsNullOrWhiteSpace(roomId))
            .WithMessage("'AcsRoomId' must not be empty.");
    }
}
