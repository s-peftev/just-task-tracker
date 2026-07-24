using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Commands;

public record JoinCallCommand(Guid CallSessionId) : IRequest<Result<CallJoinDto>>;

public class JoinCallCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    IAcsCallProvisioningService acsCallProvisioningService)
    : IRequestHandler<JoinCallCommand, Result<CallJoinDto>>
{
    public async Task<Result<CallJoinDto>> Handle(JoinCallCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByIdAsync(request.CallSessionId, ct);

        if (callSession is null)
            return Result<CallJoinDto>.Failure(GeneralErrors.NotFound);

        var userRole = await boardRepository.GetUserRoleAsync(callSession.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanJoinCall(role))
            return Result<CallJoinDto>.Failure(GeneralErrors.Forbidden);

        if (callSession.Status != CallStatus.Active)
            return Result<CallJoinDto>.Failure(CallSessionsErrors.NotActive);

        var currentUser = await userRepository.GetUserByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result<CallJoinDto>.Failure(GeneralErrors.Unauthorized);

        var token = await acsCallProvisioningService.IssueJoinTokenAsync(currentUser.Id, callSession.AcsRoomId, ct);

        return Result<CallJoinDto>.Success(new CallJoinDto(callSession.AcsRoomId, token.Token, token.ExpiresOn));
    }
}

public class JoinCallCommandValidator : AbstractValidator<JoinCallCommand>
{
    public JoinCallCommandValidator()
    {
        RuleFor(x => x.CallSessionId)
            .NotEmpty();
    }
}
