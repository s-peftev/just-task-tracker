using FluentValidation;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardMembersQuery(Guid BoardId) : PaginatedRequest, IRequest<Result<PagedList<BoardMemberDto>>>;

public class GetBoardMembersQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository)
    : IRequestHandler<GetBoardMembersQuery, Result<PagedList<BoardMemberDto>>>
{
    public async Task<Result<PagedList<BoardMemberDto>>> Handle(GetBoardMembersQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<PagedList<BoardMemberDto>>.Failure(GeneralErrors.Forbidden);

        var members = await boardRepository.GetMembersPagedAsync(
            request.BoardId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            ct);

        return Result<PagedList<BoardMemberDto>>.Success(members);
    }
}

public class GetBoardMembersQueryValidator : AbstractValidator<GetBoardMembersQuery>
{
    public GetBoardMembersQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();
    }
}
