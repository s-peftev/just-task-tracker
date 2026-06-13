using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Exceptions;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardTaskStore(IBoardApiService boardApiService) : IBoardTaskStore
{
    private CancellationTokenSource? _loadCts;

    public Guid? BoardId { get; private set; }
    public Guid? ColumnId { get; private set; }
    public Guid? TaskId { get; private set; }
    public BoardTaskDetailsDto? Task { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task LoadAsync(Guid boardId, Guid columnId, Guid taskId, CancellationToken ct = default)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loadCts = linkedCts;

        BoardId = boardId;
        ColumnId = columnId;
        TaskId = taskId;
        Task = null;
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            Task = await boardApiService.GetBoardTaskByIdAsync(boardId, columnId, taskId, linkedCts.Token);
        }
        catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
        {
            // Superseded by reset or a newer load.
        }
        catch (ApiServiceException ex)
        {
            Task = null;
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details
                ? string.Join(" ", details)
                : ex.Message;
        }
        catch (Exception ex)
        {
            Task = null;
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (ReferenceEquals(_loadCts, linkedCts))
            {
                IsLoading = false;
                linkedCts.Dispose();
                _loadCts = null;
                NotifyStateChanged();
            }
            else
            {
                linkedCts.Dispose();
            }
        }
    }

    public void UpdateTaskTitle(string title)
    {
        if (Task is not { } task)
            return;

        Task = task with { Title = title };
        NotifyStateChanged();
    }

    public void UpdateTaskDescription(string? description)
    {
        if (Task is not { } task)
            return;

        Task = task with { Description = description };
        NotifyStateChanged();
    }

    public void Reset()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        BoardId = null;
        ColumnId = null;
        TaskId = null;
        Task = null;
        IsLoading = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
