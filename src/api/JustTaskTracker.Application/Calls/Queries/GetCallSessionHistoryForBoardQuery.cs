using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Mappings;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Queries;

public record GetCallSessionHistoryForBoardQuery(Guid BoardId) : PaginatedRequest, IRequest<Result<PagedList<CallSessionHistoryDto>>>;

public class GetCallSessionHistoryForBoardQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    ICallRepository callRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetCallSessionHistoryForBoardQuery, Result<PagedList<CallSessionHistoryDto>>>
{
    public async Task<Result<PagedList<CallSessionHistoryDto>>> Handle(GetCallSessionHistoryForBoardQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } role || !BoardRolePermissions.CanViewBoard(role))
            return Result<PagedList<CallSessionHistoryDto>>.Failure(GeneralErrors.Forbidden);

        // Linked tasks/participant events/allowed users/creator projected via CallSession's
        // navigation properties in one query (CallRepository.GetClosedSessionsWithStateForBoardAsync),
        // never one round trip per session.
        var sessions = await callRepository.GetClosedSessionsWithStateForBoardAsync(
            request.BoardId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            ct);

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var items = sessions.Items.Select(session => session.ToDto(profilePhotoUrlResolver)).ToList();

        return Result<PagedList<CallSessionHistoryDto>>.Success(new PagedList<CallSessionHistoryDto>(sessions.Metadata, items));
    }
}

public class GetCallSessionHistoryForBoardQueryValidator : AbstractValidator<GetCallSessionHistoryForBoardQuery>
{
    public GetCallSessionHistoryForBoardQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();
    }
}
