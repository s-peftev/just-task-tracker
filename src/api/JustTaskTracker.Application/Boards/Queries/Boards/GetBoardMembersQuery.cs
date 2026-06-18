using FluentValidation;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Common.Searching;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardMembersQuery(Guid BoardId, TextSearchOptions<BoardMemberSearchField>? SearchOptions) 
    : PaginatedRequest, IRequest<Result<PagedList<BoardMemberDto>>>;

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
            request.SearchOptions,
            ct);

        return Result<PagedList<BoardMemberDto>>.Success(members);
    }
}

public class GetBoardMembersQueryValidator : AbstractValidator<GetBoardMembersQuery>
{
    public GetBoardMembersQueryValidator(ValidationSettings validationSettings)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        When(x => x.SearchOptions is not null, () =>
        {
            RuleFor(x => x.SearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<BoardMemberSearchField>(validationSettings.Users.MaxTextSearchLength));
        });
    }
}
