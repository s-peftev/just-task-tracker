using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Services.Abstractions.Auth;
using JustTaskTracker.WebUI.Services.Exceptions;
using System.Net;

namespace JustTaskTracker.WebUI.Services.Auth.Stores;

/// <summary>
/// Scoped store for the authenticated user's profile.
/// Implements a single-flight pattern so concurrent callers share one in-flight
/// HTTP request rather than fanning out to the API independently.
/// </summary>
internal sealed class ProfileStore(IAuthApiService authApiService) : IProfileStore, IDisposable
{
    private readonly SemaphoreSlim _sync = new(1, 1);

    // Tracks the single in-flight load Task shared across concurrent callers.
    private Task? _inFlightLoad;

    // Incremented on Reset/force refresh so stale in-flight loads cannot commit.
    private int _loadGeneration;

    public UserWithRolesDto? Profile { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool RequiresLogin { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public Task EnsureLoadedAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: false, ct);

    public Task RefreshAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: true, ct);

    public void Reset()
    {
        _loadGeneration++;
        Profile = null;
        IsLoading = false;
        IsLoaded = false;
        RequiresLogin = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    public void Dispose() => _sync.Dispose();

    // -----------------------------------------------------------------

    private async Task EnsureLoadedInternalAsync(bool forceRefresh, CancellationToken ct)
    {
        // Fast path: already loaded and no refresh requested.
        if (!forceRefresh && IsLoaded)
            return;

        Task loadTask;
        var profileClearedForRefresh = false;

        // The CancellationToken is passed only to the lock-acquisition step so
        // that a caller can give up waiting for the semaphore without affecting
        // the shared in-flight load task.
        await _sync.WaitAsync(ct);
        try
        {
            // Second check under lock handles the race between fast-path and here.
            if (!forceRefresh && IsLoaded)
                return;

            if (forceRefresh)
            {
                _loadGeneration++;
                Profile = null;
                IsLoaded = false;
                profileClearedForRefresh = true;
            }

            if (forceRefresh || _inFlightLoad is null || _inFlightLoad.IsCompleted)
                _inFlightLoad = LoadProfileAsync(_loadGeneration);

            loadTask = _inFlightLoad;
        }
        finally
        {
            _sync.Release();
        }

        if (profileClearedForRefresh)
            NotifyStateChanged();

        // WaitAsync lets each caller cancel their own wait independently.
        // The underlying loadTask keeps running for any other waiters.
        await loadTask.WaitAsync(ct);
    }

    private async Task LoadProfileAsync(int generation)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            // CancellationToken.None is intentional: LoadProfileAsync is a shared
            // task. Cancelling one caller's token must not abort the HTTP request
            // for other callers that are awaiting the same task via WaitAsync.
            var profile = await authApiService.GetCurrentUserAsync(CancellationToken.None);

            // null means user is authenticated in Entra but not yet provisioned
            // in the application database; LoginAsync provisions and returns them.
            profile ??= await authApiService.LoginAsync(CancellationToken.None);

            if (IsStaleGeneration(generation))
                return;

            Profile = profile;
            IsLoaded = true;
            RequiresLogin = false;
        }
        catch (ApiServiceException ex) when (
            ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            if (IsStaleGeneration(generation))
                return;

            // A valid MSAL token was rejected by the API (audience/scope mismatch,
            // revoked consent, etc.). Signal components to redirect to /login.
            Profile = null;
            IsLoaded = false;
            RequiresLogin = true;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            if (IsStaleGeneration(generation))
                return;

            // Network errors, 5xx, unexpected exceptions.
            Profile = null;
            IsLoaded = false;
            RequiresLogin = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (!IsStaleGeneration(generation))
                IsLoading = false;

            // Clear the in-flight reference under lock so a subsequent EnsureLoadedAsync
            // or RefreshAsync call can start a fresh load (e.g. after an error).
            // CancellationToken.None ensures cleanup always completes.
            await _sync.WaitAsync(CancellationToken.None);
            try
            {
                if (!IsStaleGeneration(generation))
                    _inFlightLoad = null;
            }
            finally
            {
                _sync.Release();
            }

            // Notify after releasing the lock to avoid potential re-entrancy issues.
            NotifyStateChanged();
        }
    }

    private bool IsStaleGeneration(int generation) => generation != _loadGeneration;

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
