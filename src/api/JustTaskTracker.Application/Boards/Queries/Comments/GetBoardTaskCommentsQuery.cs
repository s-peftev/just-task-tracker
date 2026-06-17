using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Comments;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Comments;

public record GetBoardTaskCommentsQuery(Guid BoardTaskId) : PaginatedRequest, IRequest<Result<PagedList<BoardTaskCommentDto>>>;

public class GetBoardTaskCommentsQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository)
    : IRequestHandler<GetBoardTaskCommentsQuery, Result<PagedList<BoardTaskCommentDto>>>
{
    public async Task<Result<PagedList<BoardTaskCommentDto>>> Handle(GetBoardTaskCommentsQuery request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<PagedList<BoardTaskCommentDto>>.Failure(GeneralErrors.Forbidden);

        var comments = await boardTaskCommentRepository.GetPagedByBoardTaskIdAsync(request.BoardTaskId, request.PageNumber!.Value, request.PageSize!.Value, ct);

        return Result<PagedList<BoardTaskCommentDto>>.Success(comments);
    }
}
