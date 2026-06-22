using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Boards;

internal sealed class BoardTrayOverlapService(IJSRuntime js) : IBoardTrayOverlapService
{
    private const string ModulePath = "./js/boardFooterTrayOverlap.js";

    private IJSObjectReference? _module;
    private IJSObjectReference? _handle;
    private DotNetObjectReference<BoardTrayOverlapService>? _dotNetRef;
    private readonly HashSet<Guid> _overlappingColumnIds = [];

    public bool IsAttached { get; private set; }

    public IReadOnlySet<Guid> OverlappingColumnIds => _overlappingColumnIds;

    public event Action? OverlapChanged;

    public async Task AttachAsync(ElementReference scrollBodyRef, ElementReference footerPanelRef)
    {
        if (IsAttached)
            return;

        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
        _dotNetRef ??= DotNetObjectReference.Create(this);
        _handle = await _module.InvokeAsync<IJSObjectReference>(
            "attach",
            scrollBodyRef,
            footerPanelRef,
            _dotNetRef);
        IsAttached = true;
    }

    public async Task DetachAsync()
    {
        if (_handle is not null)
        {
            await _handle.InvokeVoidAsync("dispose");
            await _handle.DisposeAsync();
            _handle = null;
        }

        IsAttached = false;

        if (_overlappingColumnIds.Count > 0)
        {
            _overlappingColumnIds.Clear();
            OverlapChanged?.Invoke();
        }
    }

    public async Task ReleaseResourcesAsync()
    {
        await DetachAsync();

        _dotNetRef?.Dispose();
        _dotNetRef = null;

        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }
    }

    public async Task RefreshAsync()
    {
        if (_handle is not null)
            await _handle.InvokeVoidAsync("refresh");
    }

    [JSInvokable]
    public void SetTrayOverlappingColumnIds(string[] columnIds)
    {
        var updatedIds = columnIds
            .Select(Guid.Parse)
            .ToHashSet();

        if (_overlappingColumnIds.SetEquals(updatedIds))
            return;

        _overlappingColumnIds.Clear();

        foreach (var id in updatedIds)
            _overlappingColumnIds.Add(id);

        OverlapChanged?.Invoke();
    }
}
