using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetActiveOwnedBoardsCountQuery : IRequest<Result<ActiveOwnedBoardsCountDto>>;

public class GetActiveOwnedBoardsCountQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IBoardRepository boardRepository)
    : IRequestHandler<GetActiveOwnedBoardsCountQuery, Result<ActiveOwnedBoardsCountDto>>
{
    public async Task<Result<ActiveOwnedBoardsCountDto>> Handle(
        GetActiveOwnedBoardsCountQuery request,
        CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<ActiveOwnedBoardsCountDto>.Failure(GeneralErrors.NotFound);

        var count = await boardRepository.CountActiveOwnedBoardsByUserIdAsync(userInfo.Id, ct);

        return Result<ActiveOwnedBoardsCountDto>.Success(new ActiveOwnedBoardsCountDto(count));
    }
}
