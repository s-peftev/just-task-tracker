using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Boards.DTOs;
using MediatR;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Searching;
using JustTaskTracker.Domain.Common.Pagination;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardsForCurrentUserQuery(TextSearchOptions<BoardSearchField>? TextSearchOptions) : PaginatedRequest, IRequest<Result<PagedList<BoardLookupDto>>>;

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
            request.TextSearchOptions,
            ct);

        return Result<PagedList<BoardLookupDto>>.Success(boards);
    }
}