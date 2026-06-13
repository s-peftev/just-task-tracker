using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Boards;

internal class BoardApiService(IBoardApi api) : IBoardApiService
{
    public async Task<PagedList<BoardLookupDto>> GetMyBoardsAsync(GetBoardsForCurrentUserRequest request, CancellationToken ct = default)
    {
        var search = string.IsNullOrWhiteSpace(request.TextSearchOptions?.Search)
            ? null
            : request.TextSearchOptions.Search;

        var response = await api.GetMyAsync(
            request.PageNumber!.Value,
            request.PageSize!.Value,
            search,
            ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardDetailsDto> GetBoardByIdAsync(Guid boardId, CancellationToken ct = default)
    {
        var response = await api.GetByIdAsync(boardId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardDetailsDto> CreateBoardAsync(string name, CancellationToken ct = default)
    {
        var response = await api.CreateAsync(new SaveBoardRequest(name), ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task UpdateBoardAsync(Guid boardId, string name, CancellationToken ct = default)
    {
        var response = await api.UpdateAsync(boardId, new SaveBoardRequest(name), ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task UpdateColumnAsync(Guid boardId, Guid columnId, string name, CancellationToken ct = default)
    {
        var response = await api.UpdateColumnAsync(boardId, columnId, new SaveColumnRequest(name), ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task DeleteColumnAsync(Guid boardId, Guid columnId, DeleteColumnRequest request, CancellationToken ct = default)
    {
        var response = await api.DeleteColumnAsync(boardId, columnId, request, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task DeleteBoardAsync(Guid boardId, CancellationToken ct = default)
    {
        var response = await api.DeleteAsync(boardId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task<ColumnDto> CreateColumnAsync(Guid boardId, string name, CancellationToken ct = default)
    {
        var response = await api.CreateColumnAsync(boardId, new SaveColumnRequest(name), ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task ReorderColumnAsync(
        Guid boardId,
        Guid columnId,
        int position,
        CancellationToken ct = default)
    {
        var response = await api.ReorderColumnAsync(boardId, columnId, new ReorderColumnPositionRequest(position), ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task ReorderTaskAsync(
        Guid boardId,
        Guid targetColumnId,
        Guid taskId,
        int position,
        CancellationToken ct = default)
    {
        var response = await api.ReorderTaskAsync(
            boardId,
            targetColumnId,
            taskId,
            new ReorderTaskPositionRequest(position),
            ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task<TaskLookupDto> CreateTaskAsync(
        Guid boardId,
        Guid columnId,
        string title,
        CancellationToken ct = default)
    {
        var response = await api.CreateTaskAsync(boardId, columnId, new SaveTaskRequest(title), ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardTaskDetailsDto> GetBoardTaskByIdAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        CancellationToken ct = default)
    {
        var response = await api.GetTaskByIdAsync(boardId, columnId, taskId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task UpdateBoardTaskAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        UpdateBoardTaskRequest request,
        CancellationToken ct = default)
    {
        var response = await api.UpdateTaskAsync(boardId, columnId, taskId, request, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }
}
