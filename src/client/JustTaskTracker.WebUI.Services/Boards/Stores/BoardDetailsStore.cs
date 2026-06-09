using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Exceptions;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardDetailsStore(IBoardApiService boardApiService) : IBoardDetailsStore
{
    public Guid? BoardId { get; private set; }
    public BoardDetailsDto? Board { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task LoadAsync(Guid boardId, CancellationToken ct = default)
    {
        if (BoardId == boardId && Board is not null && !IsLoading)
            return;

        BoardId = boardId;
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            Board = await boardApiService.GetBoardByIdAsync(boardId, ct);
        }
        catch (ApiServiceException ex)
        {
            Board = null;
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details
                ? string.Join(" ", details)
                : ex.Message;
        }
        catch (Exception ex)
        {
            Board = null;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public async Task<ColumnDto> CreateColumnAsync(string name, CancellationToken ct = default)
    {
        if (BoardId is not { } boardId || Board is null)
            throw new InvalidOperationException("Board details are not loaded.");

        var column = await boardApiService.CreateColumnAsync(boardId, name, ct);
        AddColumn(column);

        return column;
    }

    public void UpdateBoardName(string name)
    {
        if (Board is null)
            return;

        Board = Board with { Name = name };
        NotifyStateChanged();
    }

    public void UpdateColumnName(Guid columnId, string name)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Select(column => column.Id == columnId ? column with { Name = name } : column)
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    public void Reset()
    {
        BoardId = null;
        Board = null;
        IsLoading = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    private void AddColumn(ColumnDto column)
    {
        if (Board is null)
            return;

        var columns = Board.Columns
            .Append(column)
            .OrderBy(c => c.Position)
            .ToList();

        Board = Board with { Columns = columns };
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
