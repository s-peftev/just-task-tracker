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

    Task ReorderTaskAsync(
        Guid boardId,
        Guid targetColumnId,
        Guid taskId,
        int position,
        CancellationToken ct = default);

    Task<TaskLookupDto> CreateTaskAsync(Guid boardId, Guid columnId, string title, CancellationToken ct = default);

    Task<BoardTaskDetailsDto> GetBoardTaskByIdAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        CancellationToken ct = default);

    Task UpdateBoardTaskTitleAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        string title,
        CancellationToken ct = default);

    Task UpdateBoardTaskDescriptionAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        string? description,
        CancellationToken ct = default);

    Task UpdateBoardTaskAssigneeAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        Guid? assigneeId,
        CancellationToken ct = default);

    Task DeleteBoardTaskAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        CancellationToken ct = default);
}
