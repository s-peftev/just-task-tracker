using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Commands;

// INTERIM (Story 1.1): creator-only, direct status write. Superseded by Story 1.2's
// Event-Grid pipeline and converted to a trigger + extended to Owner/Admin in Story 1.6 --
// see the architecture memlog for the reconciliation note (AD-12/AD-15).
public record EndCallCommand(Guid CallSessionId) : IRequest<Result>;

public class EndCallCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    ICallRepository callRepository,
    IAcsCallProvisioningService acsCallProvisioningService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
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

        if (callSession.CreatedByUserId != currentUser.Id)
            return Result.Failure(GeneralErrors.Forbidden);

        if (callSession.Status != CallStatus.Active)
            return Result.Failure(CallSessionsErrors.NotActive);

        callSession.Status = CallStatus.Closed;
        callSession.EndedAtUtc = dateTimeProvider.UtcNow;
        callRepository.Update(callSession);

        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await acsCallProvisioningService.DeleteRoomAsync(callSession.AcsRoomId, ct);
        }
        catch
        {
            // Best-effort cleanup: the session is already reliably marked Closed regardless.
        }

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
