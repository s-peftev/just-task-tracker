using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardRepository : IRepository<Board, Guid>
{
    void AddMember(BoardMember member);

    void RemoveMember(BoardMember member);

    Task<BoardMember?> GetMemberAsync(Guid boardId, Guid userId, CancellationToken ct = default);

    Task<BoardMember?> GetMemberByAzureAOIAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardMemberRole?> GetUserRoleAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<(Board? Board, BoardMemberRole? UserRole)> GetBoardWithUserRoleAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardDetailsReadModel?> GetBoardDetailsByIdAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<PagedList<BoardLookupReadModel>> GetBoardsByUserAzureAOIAsync(
        Guid azureAdObjectId,
        int pageNumber,
        int pageSize,
        TextSearchOptions<BoardSearchField>? searchOptions = null,
        bool? isArchived = null,
        CancellationToken ct = default);

    Task<bool> IsBoardMemberAsync(Guid boardId, Guid userId, CancellationToken ct = default);

    Task<bool> IsArchivedAsync(Guid boardId, CancellationToken ct = default);

    Task<PagedList<BoardMemberReadModel>> GetMembersInfoPagedAsync(
        Guid boardId,
        int pageNumber,
        int pageSize,
        TextSearchOptions<BoardMemberSearchField>? searchOptions = null,
        CancellationToken ct = default);
}