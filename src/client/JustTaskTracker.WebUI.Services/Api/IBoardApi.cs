using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IBoardApi
{
    [Get("/api/boards")]
    Task<IApiResponse<ApiEnvelope<PagedList<BoardLookupDto>>>> GetMyAsync(
        int pageNumber,
        int pageSize,
        [AliasAs("TextSearchOptions.Search")] string? textSearchOptionsSearch = null,
        CancellationToken ct = default);

    [Get("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> GetByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/boards")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> CreateAsync([Body] SaveBoardRequest request, CancellationToken ct = default);

    [Put("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateAsync(Guid id, [Body] SaveBoardRequest request, CancellationToken ct = default);

    [Delete("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<object>>> DeleteAsync(Guid id, CancellationToken ct = default);

    [Post("/api/boards/{boardId}/columns")]
    Task<IApiResponse<ApiEnvelope<ColumnDto>>> CreateColumnAsync(Guid boardId, [Body] SaveColumnRequest request, CancellationToken ct = default);

    [Put("/api/boards/{boardId}/columns/{columnId}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateColumnAsync(Guid boardId, Guid columnId, [Body] SaveColumnRequest request, CancellationToken ct = default);

    [Delete("/api/boards/{boardId}/columns/{columnId}")]
    Task<IApiResponse<ApiEnvelope<object>>> DeleteColumnAsync(Guid boardId, Guid columnId, [Body] DeleteColumnRequest request, CancellationToken ct = default);

    [Put("/api/boards/{boardId}/columns/{columnId}/position")]
    Task<IApiResponse<ApiEnvelope<object>>> ReorderColumnAsync(Guid boardId, Guid columnId, [Body] ReorderColumnPositionRequest request, CancellationToken ct = default);

    [Put("/api/boards/{boardId}/columns/{columnId}/tasks/{taskId}/position")]
    Task<IApiResponse<ApiEnvelope<object>>> ReorderTaskAsync(Guid boardId, Guid columnId, Guid taskId, [Body] ReorderTaskPositionRequest request, CancellationToken ct = default);

    [Post("/api/boards/{boardId}/columns/{columnId}/tasks")]
    Task<IApiResponse<ApiEnvelope<TaskLookupDto>>> CreateTaskAsync(Guid boardId, Guid columnId, [Body] SaveTaskRequest request, CancellationToken ct = default);

    [Get("/api/boards/{boardId}/columns/{columnId}/tasks/{taskId}")]
    Task<IApiResponse<ApiEnvelope<BoardTaskDetailsDto>>> GetTaskByIdAsync(Guid boardId, Guid columnId, Guid taskId, CancellationToken ct = default);

    [Patch("/api/boards/{boardId}/columns/{columnId}/tasks/{taskId}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateTaskTitleAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        [Body] UpdateBoardTaskTitleRequest request,
        CancellationToken ct = default);

    [Patch("/api/boards/{boardId}/columns/{columnId}/tasks/{taskId}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateTaskDescriptionAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        [Body] UpdateBoardTaskDescriptionRequest request,
        CancellationToken ct = default);

    [Patch("/api/boards/{boardId}/columns/{columnId}/tasks/{taskId}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateTaskAssigneeAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        [Body] UpdateBoardTaskAssigneeRequest request,
        CancellationToken ct = default);

    [Delete("/api/boards/{boardId}/columns/{columnId}/tasks/{taskId}")]
    Task<IApiResponse<ApiEnvelope<object>>> DeleteTaskAsync(
        Guid boardId,
        Guid columnId,
        Guid taskId,
        CancellationToken ct = default);
}
