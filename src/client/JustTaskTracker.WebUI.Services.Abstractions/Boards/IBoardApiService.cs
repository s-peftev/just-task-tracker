using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

public interface IBoardApiService
{
    Task<PagedList<BoardLookupDto>> GetMyBoardsAsync(GetBoardsForCurrentUserRequest request, CancellationToken ct = default);
    Task<BoardDetailsDto> GetBoardByIdAsync(Guid boardId, CancellationToken ct = default);
    Task<BoardDetailsDto> CreateBoardAsync(string name, CancellationToken ct = default);
    Task UpdateBoardAsync(Guid boardId, string name, CancellationToken ct = default);
    Task UpdateColumnAsync(Guid boardId, Guid columnId, string name, CancellationToken ct = default);
    Task DeleteBoardAsync(Guid boardId, CancellationToken ct = default);
    Task DeleteColumnAsync(Guid boardId, Guid columnId, DeleteColumnRequest request, CancellationToken ct = default);
    Task<ColumnDto> CreateColumnAsync(Guid boardId, string name, CancellationToken ct = default);

    Task ReorderColumnAsync(
        Guid boardId,
        Guid columnId,
        int position,
        CancellationToken ct = default);

    Task<TaskLookupDto> CreateTaskAsync(Guid boardId, Guid columnId, string title, CancellationToken ct = default);
}
