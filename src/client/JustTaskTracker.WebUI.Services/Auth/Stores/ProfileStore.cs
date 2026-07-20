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
        EnsureLoadedInternalAsync(forceRefresh: false, useLogin: false, ct);

    public Task RefreshAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: true, useLogin: false, ct);

    public Task LoginAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: true, useLogin: true, ct);

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

    public void SetProfilePhotoUrl(string profilePhotoUrl)
    {
        if (Profile is null)
            return;

        Profile = Profile with { ProfilePhotoUrl = profilePhotoUrl };
        NotifyStateChanged();
    }

    public void ClearProfilePhotoUrl()
    {
        if (Profile is null)
            return;

        Profile = Profile with { ProfilePhotoUrl = null };
        NotifyStateChanged();
    }

    public void Dispose() => _sync.Dispose();

    // -----------------------------------------------------------------

    private async Task EnsureLoadedInternalAsync(bool forceRefresh, bool useLogin, CancellationToken ct)
    {
        // Fast path: already loaded and no refresh/login requested.
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
            {
                _inFlightLoad = useLogin
                    ? LoadViaLoginAsync(_loadGeneration)
                    : LoadViaMeAsync(_loadGeneration);
            }

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

    /// <summary>
    /// Session restore path: GET /auth/me. On 404 (user not provisioned), one-shot
    /// POST /auth/login so a lost pending-login flag still provisions the account.
    /// </summary>
    private async Task LoadViaMeAsync(int generation)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            // CancellationToken.None is intentional: shared in-flight task must not
            // abort when one caller's WaitAsync token is cancelled.
            // null = Entra-authenticated but not yet provisioned in the app DB.
            var profile = await authApiService.GetCurrentUserAsync(CancellationToken.None)
                ?? await authApiService.LoginAsync(CancellationToken.None);

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

            Profile = null;
            IsLoaded = false;
            RequiresLogin = true;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            if (IsStaleGeneration(generation))
                return;

            Profile = null;
            IsLoaded = false;
            RequiresLogin = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            await CompleteLoadAsync(generation);
        }
    }

    /// <summary>
    /// Interactive login / switch / mismatch path: POST /auth/login (provision + role sync).
    /// </summary>
    private async Task LoadViaLoginAsync(int generation)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var profile = await authApiService.LoginAsync(CancellationToken.None);

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

            Profile = null;
            IsLoaded = false;
            RequiresLogin = true;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            if (IsStaleGeneration(generation))
                return;

            Profile = null;
            IsLoaded = false;
            RequiresLogin = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            await CompleteLoadAsync(generation);
        }
    }

    private async Task CompleteLoadAsync(int generation)
    {
        if (!IsStaleGeneration(generation))
            IsLoading = false;

        // Clear the in-flight reference under lock so a subsequent EnsureLoadedAsync,
        // RefreshAsync, or LoginAsync can start a fresh load (e.g. after an error).
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

        NotifyStateChanged();
    }

    private bool IsStaleGeneration(int generation) => generation != _loadGeneration;

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
