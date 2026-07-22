using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Billing.Enums;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record CreateBoardCommand(string Name) : IRequest<Result<BoardDetailsDto>>, IRequirePlanLimit
{
    public PlanLimitKind Limit => PlanLimitKind.Boards;

    public Guid? BoardId => null;
}

public class CreateBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork,
    IEntitlementService entitlementService)
    : IRequestHandler<CreateBoardCommand, Result<BoardDetailsDto>>
{
    public async Task<Result<BoardDetailsDto>> Handle(CreateBoardCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.Unauthorized);

        var board = new Board { Name = request.Name };
        boardRepository.Add(board);

        boardRepository.AddMember(new BoardMember
        {
            BoardId = board.Id,
            UserId = currentUserInfo.Id,
            Role = BoardMemberRole.Owner
        });

        await unitOfWork.SaveChangesAsync(ct);

        var ownerEntitlements = await entitlementService.GetEntitlementsAsync(currentUserInfo.Id, ct);

        return Result<BoardDetailsDto>.Success(new BoardDetailsDto(
            board.Id,
            board.Name,
            board.CreatedAtUtc,
            board.IsArchived,
            BoardMemberRole.Owner,
            [],
            BoardExportStatus.None,
            ownerEntitlements.Limits.ToBoardLimits()));
    }
}

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'Name' must not be empty.")
            .MaximumLength(BoardFieldLengths.MaxNameLength);
    }
}
