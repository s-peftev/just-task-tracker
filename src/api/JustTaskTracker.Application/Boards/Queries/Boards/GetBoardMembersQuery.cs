using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
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
    IBoardRepository boardRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetBoardMembersQuery, Result<PagedList<BoardMemberDto>>>
{
    public async Task<Result<PagedList<BoardMemberDto>>> Handle(GetBoardMembersQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<PagedList<BoardMemberDto>>.Failure(GeneralErrors.Forbidden);

        var membersInfo = await boardRepository.GetMembersInfoPagedAsync(
            request.BoardId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            request.SearchOptions,
            ct);

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id);

        var members = new PagedList<BoardMemberDto>(
            membersInfo.Metadata,
            membersInfo.Items.Select(member => member.ToDto(profilePhotoUrlResolver)));

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
                .SetValidator(new TextSearchOptionsValidator<BoardMemberSearchField>(validationSettings.Users!.MaxTextSearchLength));
        });
    }
}
