using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDetailsDto>>;

public class GetBoardByIdQueryHandler(ICurrentUserAccessor currentUserAccessor, IBoardRepository boardRepository)
    : IRequestHandler<GetBoardByIdQuery, Result<BoardDetailsDto>>
{
    public async Task<Result<BoardDetailsDto>> Handle(GetBoardByIdQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<BoardDetailsDto>.Failure(GeneralErrors.Forbidden);

        var board = await boardRepository.GetBoardDetailsByIdAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (board is null)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.NotFound);

        return Result<BoardDetailsDto>.Success(board);
    }
}
