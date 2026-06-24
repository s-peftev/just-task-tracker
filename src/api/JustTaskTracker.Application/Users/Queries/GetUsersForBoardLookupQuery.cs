using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Common.Searching;
using MediatR;

namespace JustTaskTracker.Application.Users.Queries;

public record GetUsersForBoardLookupQuery(Guid BoardId, TextSearchOptions<UserSearchField>? SearchOptions)
    : PaginatedRequest, IRequest<Result<PagedList<UserForBoardLookupDto>>>;

public class GetUsersForBoardLookupQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUserRepository userRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetUsersForBoardLookupQuery, Result<PagedList<UserForBoardLookupDto>>>
{
    public async Task<Result<PagedList<UserForBoardLookupDto>>> Handle(GetUsersForBoardLookupQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageMembers(authorizedRole))
            return Result<PagedList<UserForBoardLookupDto>>.Failure(GeneralErrors.Forbidden);

        var usersReadModel = await userRepository.GetPagedUserForBoardLookup(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            request.SearchOptions,
            ct);

        Func<UserForBoardLookupReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var users = new PagedList<UserForBoardLookupDto>(
            usersReadModel.Metadata,
            usersReadModel.Items.Select(user => user.ToDto(profilePhotoUrlResolver)));

        return Result<PagedList<UserForBoardLookupDto>>.Success(users);
    }
}

public class GetUsersForBoardLookupQueryValidator : AbstractValidator<GetUsersForBoardLookupQuery>
{
    public GetUsersForBoardLookupQueryValidator(ValidationSettings validationSettings)
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        When(x => x.SearchOptions is not null, () =>
        {
            RuleFor(x => x.SearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<UserSearchField>(validationSettings.Users!.MaxTextSearchLength));
        });
    }
}
