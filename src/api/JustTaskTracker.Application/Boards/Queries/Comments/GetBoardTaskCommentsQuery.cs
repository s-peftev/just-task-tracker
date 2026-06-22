using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Auth.DTOs;
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
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetBoardTaskCommentsQuery, Result<PagedList<BoardTaskCommentDto>>>
{
    public async Task<Result<PagedList<BoardTaskCommentDto>>> Handle(GetBoardTaskCommentsQuery request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<PagedList<BoardTaskCommentDto>>.Failure(GeneralErrors.Forbidden);

        var commentsInfo = await boardTaskCommentRepository.GetPagedInfoByBoardTaskIdAsync(request.BoardTaskId, request.PageNumber!.Value, request.PageSize!.Value, ct);

        var comments = new PagedList<BoardTaskCommentDto>(
            commentsInfo.Metadata,
            commentsInfo.Items.Select(c => new BoardTaskCommentDto(
                c.Id,
                c.Body,
                c.CreatedAtUtc,
                new UserDto(
                    c.Author.Id,
                    c.Author.Email,
                    c.Author.DisplayName,
                    c.Author.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(c.Author.Id)),
                c.LastModifiedAtUtc)));

        return Result<PagedList<BoardTaskCommentDto>>.Success(comments);
    }
}
