using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Helpers;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;

namespace JustTaskTracker.Persistence.Auth.Repositories;

public class UserRepository(JustTaskTrackerDbContext context) : Repository<User, Guid>(context), IUserRepository
{
    public async Task<UserDto?> GetUserDtoByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(u => u.AzureAdObjectId == azureAdObjectId)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.DisplayName
            ))
            .FirstOrDefaultAsync(ct);

    public async Task<User?> GetUserByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId, ct);

    public async Task<PagedList<UserDto>> GetPagedUserDto(
        Guid exludeUserAzureAOI,
        int pageNumber,
        int pageSize,
        TextSearchOptions<UserSearchField>? searchOptions = null,
        CancellationToken ct = default)
    {
        var fields = SearchFieldsResolver.Resolve(searchOptions?.SearchIn, UserSearchFields.Map);

        return await _dbSet
            .Where(u => u.AzureAdObjectId != exludeUserAzureAOI)
            .ApplyTextSearch(searchOptions?.Search, fields)
            .OrderBy(u => u.DisplayName)
            .ToPagedAsync(
                x => new UserDto(
                    x.Id,
                    x.Email,
                    x.DisplayName),
                pageSize,
                pageNumber,
                ct);
    }
}
