using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
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

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var board = new Board { Name = request.Name };
            boardRepository.Add(board);

            boardRepository.AddMember(new BoardMember
            {
                BoardId = board.Id,
                UserId = user.Id,
                Role = BoardMemberRole.Owner
            });

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);

            return Result<BoardDetailsDto>.Success(new BoardDetailsDto(
                board.Id,
                board.Name,
                board.CreatedAtUtc,
                BoardMemberRole.Owner,
                [new UserDto(user.Id, user.Email, user.DisplayName)],
                []));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
