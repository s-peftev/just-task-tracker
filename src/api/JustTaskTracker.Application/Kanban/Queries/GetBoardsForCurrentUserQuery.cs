using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Kanban.Repositories;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Kanban.DTOs;
using MediatR;

namespace JustTaskTracker.Application.Kanban.Queries;

public record GetBoardsForCurrentUserQuery : PaginatedRequest, IRequest<Result<PagedList<BoardLookupDto>>>;

public class GetBoardsForCurrentUserQueryHandler(
    ICurrentUserAccessor currentUser,
    IBoardRepository boardRepository) 
    : IRequestHandler<GetBoardsForCurrentUserQuery, Result<PagedList<BoardLookupDto>>>
{
    public async Task<Result<PagedList<BoardLookupDto>>> Handle(GetBoardsForCurrentUserQuery request, CancellationToken ct)
    {
        var boards = await boardRepository.GetBoardsByUserAzureAOIAsync(currentUser.AzureAdObjectId, request.PageNumber, request.PageSize, ct);

        return Result<PagedList<BoardLookupDto>>.Success(boards);
    }
}