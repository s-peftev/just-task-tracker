using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Boards.DTOs;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardsForCurrentUserQuery : PaginatedRequest, IRequest<Result<PagedList<BoardLookupDto>>>;

public class GetBoardsForCurrentUserQueryHandler(
    ICurrentUserAccessor currentUser,
    IBoardRepository boardRepository) 
    : IRequestHandler<GetBoardsForCurrentUserQuery, Result<PagedList<BoardLookupDto>>>
{
    public async Task<Result<PagedList<BoardLookupDto>>> Handle(GetBoardsForCurrentUserQuery request, CancellationToken ct)
    {
        var boards = await boardRepository.GetBoardsByUserAzureAOIAsync(
            currentUser.AzureAdObjectId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            ct);

        return Result<PagedList<BoardLookupDto>>.Success(boards);
    }
}