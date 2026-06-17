using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Helpers;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PagedList<UserForBoardLookupDto>> GetPagedUserForBoardLookupDto(
        Guid boardId,
        Guid excludeUserAzureAOI,
        int pageNumber,
        int pageSize,
        TextSearchOptions<UserSearchField>? searchOptions = null,
        CancellationToken ct = default)
    {
        var fields = SearchFieldsResolver.Resolve(searchOptions?.SearchIn, UserSearchFields.Map);

        return await _dbSet
            .Where(u => u.AzureAdObjectId != excludeUserAzureAOI)
            .ApplyTextSearch(searchOptions?.Search, fields)
            .OrderBy(u => u.DisplayName)
            .ToPagedAsync(
                u => new UserForBoardLookupDto(
                    u.Id,
                    u.Email,
                    u.DisplayName,
                    _context.BoardMembers
                        .Where(m => m.BoardId == boardId && m.UserId == u.Id)
                        .Select(m => (BoardMemberRole?)m.Role)
                        .FirstOrDefault()),
                pageNumber,
                pageSize,
                ct);
    }
}
