using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDetailsDto>>;

public class GetBoardByIdQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository)
    : IRequestHandler<GetBoardByIdQuery, Result<BoardDetailsDto>>
{
    public async Task<Result<BoardDetailsDto>> Handle(GetBoardByIdQuery request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanViewBoard) is { } failure)
            return Result<BoardDetailsDto>.Failure(failure.Error);

        var board = await boardRepository.GetBoardDetailsByIdAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.NotFound);

        return Result<BoardDetailsDto>.Success(board);
    }
}
