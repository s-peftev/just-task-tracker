using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Auth.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Services.Abstractions.Users;
using JustTaskTracker.WebUI.Services.Api;

namespace JustTaskTracker.WebUI.Services.Users;

internal sealed class UsersApiService(IUsersApi api) : IUsersApiService
{
    public async Task<PagedList<UserForBoardLookupDto>> GetUsersForBoardLookupAsync(
        Guid boardId,
        GetUsersForBoardLookupRequest request,
        CancellationToken ct = default)
    {
        var search = string.IsNullOrWhiteSpace(request.SearchOptions?.Search)
            ? null
            : request.SearchOptions.Search;

        var response = await api.GetForBoardLookupAsync(
            boardId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            search,
            ct);

        return ApiResponseGuard.Unwrap(response);
    }
}
