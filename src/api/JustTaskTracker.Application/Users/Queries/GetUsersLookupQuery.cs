using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Searching;
using MediatR;

namespace JustTaskTracker.Application.Users.Queries;

public record GetUsersLookupQuery(TextSearchOptions<UserSearchField>? SearchOptions) : PaginatedRequest, IRequest<Result<PagedList<UserDto>>>;

public class GetUsersLookupQueryHandler(ICurrentUserAccessor currentUserAccessor, IUserRepository userRepository)
    : IRequestHandler<GetUsersLookupQuery, Result<PagedList<UserDto>>>
{
    public async Task<Result<PagedList<UserDto>>> Handle(GetUsersLookupQuery request, CancellationToken ct)
    {
        var users = await userRepository.GetPagedUserDto(
            currentUserAccessor.AzureAdObjectId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            request.SearchOptions,
            ct);

        return Result<PagedList<UserDto>>.Success(users);
    }
}

public class GetUsersLookupQueryValidator : AbstractValidator<GetUsersLookupQuery>
{
    public GetUsersLookupQueryValidator(ValidationSettings validationSettings)
    {
        var maxUserTextSearchLength = validationSettings.Users.MaxTextSearchLength;

        When(x => x.SearchOptions is not null, () =>
        {
            RuleFor(x => x.SearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<UserSearchField>(maxUserTextSearchLength));
        });
    }
}