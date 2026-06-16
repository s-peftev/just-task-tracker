using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using Refit;

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

    public async Task<BoardTaskPreviewDto> CreateTaskAsync(
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

    public async Task UpdateBoardTaskTitleAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        string title,
        CancellationToken ct = default)
    {
        var response = await api.UpdateTaskTitleAsync(
            boardId,
            columnId,
            taskId,
            new UpdateBoardTaskTitleRequest(title),
            ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task UpdateBoardTaskDescriptionAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        string? description,
        CancellationToken ct = default)
    {
        var response = await api.UpdateTaskDescriptionAsync(
            boardId,
            columnId,
            taskId,
            new UpdateBoardTaskDescriptionRequest(description),
            ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task UpdateBoardTaskAssigneeAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        Guid? assigneeId,
        CancellationToken ct = default)
    {
        var response = await api.UpdateTaskAssigneeAsync(
            boardId,
            columnId,
            taskId,
            new UpdateBoardTaskAssigneeRequest(assigneeId),
            ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task DeleteBoardTaskAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        CancellationToken ct = default)
    {
        var response = await api.DeleteTaskAsync(boardId, columnId, taskId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task<BoardTaskAttachmentDto> UploadBoardTaskAttachmentAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var streamPart = new StreamPart(content, fileName, contentType);

        var response = await api.UploadTaskAttachmentAsync(
            boardId,
            columnId,
            taskId,
            streamPart,
            ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task DeleteBoardTaskAttachmentAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        Guid attachmentId,
        CancellationToken ct = default)
    {
        var response = await api.DeleteTaskAttachmentAsync(boardId, columnId, taskId, attachmentId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task<BoardTaskAttachmentFile> DownloadBoardTaskAttachmentAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        Guid attachmentId,
        string fileName,
        CancellationToken ct = default)
    {
        using var response = await api.DownloadTaskAttachmentAsync(
            boardId,
            columnId,
            taskId,
            attachmentId,
            ct);

        var (content, contentType) = await ApiResponseGuard.ReadBinarySuccessAsync(response, ct);

        return new BoardTaskAttachmentFile(content, contentType, fileName);
    }

    public async Task<PagedList<BoardTaskCommentDto>> GetBoardTaskCommentsAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var response = await api.GetTaskCommentsAsync(
            boardId,
            columnId,
            taskId,
            pageNumber,
            pageSize,
            ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardTaskCommentDto> CreateBoardTaskCommentAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        string body,
        CancellationToken ct = default)
    {
        var response = await api.CreateTaskCommentAsync(
            boardId,
            columnId,
            taskId,
            new CreateBoardTaskCommentRequest(body),
            ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task UpdateBoardTaskCommentAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        Guid commentId,
        string body,
        CancellationToken ct = default)
    {
        var response = await api.UpdateTaskCommentAsync(
            boardId,
            columnId,
            taskId,
            commentId,
            new UpdateBoardTaskCommentRequest(body),
            ct);

        ApiResponseGuard.EnsureSuccess(response);
    }
}
