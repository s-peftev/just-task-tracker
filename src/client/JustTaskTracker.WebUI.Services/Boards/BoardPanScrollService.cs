using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Boards;

internal sealed class BoardPanScrollService(IJSRuntime js) : IBoardPanScrollService
{
    private const string ModulePath = "./js/boardPanScroll.js";

    private IJSObjectReference? _module;
    private IJSObjectReference? _handle;

    public bool IsAttached { get; private set; }

    public async Task AttachAsync(ElementReference contentRef, ElementReference scrollRef)
    {
        if (IsAttached)
            return;

        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
        _handle = await _module.InvokeAsync<IJSObjectReference>("attach", contentRef, scrollRef);
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

        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }

        IsAttached = false;
    }
}
