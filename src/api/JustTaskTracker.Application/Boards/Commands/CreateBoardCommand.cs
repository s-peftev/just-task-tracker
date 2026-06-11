using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record CreateBoardCommand(string Name) : IRequest<Result<BoardDetailsDto>>;

public class CreateBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBoardCommand, Result<BoardDetailsDto>>
{
    public async Task<Result<BoardDetailsDto>> Handle(CreateBoardCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetUserByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (user is null)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.Unauthorized);

        var board = new Board { Name = request.Name };
        boardRepository.Add(board);

        boardRepository.AddMember(new BoardMember
        {
            BoardId = board.Id,
            UserId = user.Id,
            Role = BoardMemberRole.Owner
        });

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardDetailsDto>.Success(new BoardDetailsDto(
            board.Id,
            board.Name,
            board.CreatedAtUtc,
            BoardMemberRole.Owner,
            [new UserDto(user.Id, user.Email, user.DisplayName)],
            []));
    }
}

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'Name' must not be empty.")
            .MaximumLength(100);
    }
}