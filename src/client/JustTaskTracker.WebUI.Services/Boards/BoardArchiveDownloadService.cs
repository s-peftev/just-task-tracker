using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Boards;

internal sealed class BoardArchiveDownloadService(
    IBoardApiService boardApiService,
    IJSRuntime js) : IBoardArchiveDownloadService
{
    private const string ModulePath = "./js/fileDownload.js";

    private IJSObjectReference? _module;

    public async Task DownloadAsync(Guid boardId, CancellationToken ct = default)
    {
        var archive = await boardApiService.GetBoardArchiveDownloadAsync(boardId, ct);

        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);

        await _module.InvokeVoidAsync(
            "downloadFromUrl",
            ct,
            archive.DownloadUrl.ToString(),
            archive.FileName);
    }
}
