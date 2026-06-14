using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardTaskCommentsQuery(
    Guid BoardId,
    Guid ColumnId,
    Guid BoardTaskId) : PaginatedRequest, IRequest<Result<PagedList<BoardTaskCommentDto>>>;

public class GetBoardTaskCommentsQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository)
    : IRequestHandler<GetBoardTaskCommentsQuery, Result<PagedList<BoardTaskCommentDto>>>
{
    public async Task<Result<PagedList<BoardTaskCommentDto>>> Handle(
        GetBoardTaskCommentsQuery request,
        CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanViewBoard) is { } failure)
            return Result<PagedList<BoardTaskCommentDto>>.Failure(failure.Error);

        if (!await boardTaskRepository.ExistsByBoardIdAndColumnIdAndIdAsync(
                request.BoardId,
                request.ColumnId,
                request.BoardTaskId,
                ct))
            return Result<PagedList<BoardTaskCommentDto>>.Failure(GeneralErrors.NotFound);

        var comments = await boardTaskCommentRepository.GetPagedByBoardIdAndColumnIdAndTaskIdAsync(
            request.BoardId,
            request.ColumnId,
            request.BoardTaskId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            ct);

        return Result<PagedList<BoardTaskCommentDto>>.Success(comments);
    }
}
