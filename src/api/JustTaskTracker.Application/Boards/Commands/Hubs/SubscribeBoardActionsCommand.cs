using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Hubs;

public record SubscribeBoardActionsCommand(Guid BoardId) : IRequest<Result>, IRequireActiveBoard;

public class SubscribeBoardActionsCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository)
    : IRequestHandler<SubscribeBoardActionsCommand, Result>
{
    public async Task<Result> Handle(
        SubscribeBoardActionsCommand request,
        CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        return Result.Success();
    }
}

public class SubscribeBoardActionsCommandValidator : AbstractValidator<SubscribeBoardActionsCommand>
{
    public SubscribeBoardActionsCommandValidator(ValidationSettings validationSettings)
    {
        RuleFor(x => x.BoardId)
            .NotNull();
    }
}
