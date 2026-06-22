using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;

namespace JustTaskTracker.Application.Auth.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<UserReadModel?> GetUserInfoByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default);

    Task<User?> GetUserByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default);

    Task<PagedList<UserForBoardLookupReadModel>> GetPagedUserForBoardLookup(
        Guid boardId,
        Guid excludeUserAzureAOI,
        int pageNumber,
        int pageSize,
        TextSearchOptions<UserSearchField>? searchOptions = null,
        CancellationToken ct = default);
}
